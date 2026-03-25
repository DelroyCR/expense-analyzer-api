using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using ExpenseAnalyzer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExpenseAnalyzer.IntegrationTests;

public class ImportEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ImportEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ImportCsv_ShouldImportValidFile_WhenRequestIsValid()
    {
        // Arrange - register and login
        var email = $"import_{Guid.NewGuid():N}@example.com";
        var password = "Password123!";

        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = password
        };

        var loginRequest = new LoginRequestDto
        {
            Email = email,
            Password = password
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginBody);
        Assert.False(string.IsNullOrWhiteSpace(loginBody!.Token));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.Token);

        var meResponse = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var meBody = await meResponse.Content.ReadFromJsonAsync<MeResponseDto>();
        Assert.NotNull(meBody);

        var userId = meBody!.UserId;

        var csv = """
    Date,Description,Amount
    2026-01-10,Amazon Purchase,25.50
    2026-01-11,Uber Ride,10.00
    """;

        using var multipartContent = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csv));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");

        multipartContent.Add(fileContent, "File", "valid-import.csv");

        // Act
        var response = await _client.PostAsync("/api/imports", multipartContent);

        // Assert - HTTP response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ImportCsvResponseDto>();
        Assert.NotNull(body);

        Assert.NotEqual(Guid.Empty, body!.ImportJobId);
        Assert.Equal("valid-import.csv", body.FileName);
        Assert.Equal(2, body.ImportedCount);
        Assert.Equal(0, body.SkippedCount);
        Assert.Empty(body.Errors);

        // Assert - data persisted in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ExpenseAnalyzerDbContext>();

        var importJob = await dbContext.ImportJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == body.ImportJobId && x.UserId == userId);

        Assert.NotNull(importJob);

        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.ImportJobId == body.ImportJobId && x.UserId == userId)
            .OrderBy(x => x.Description)
            .ToListAsync();

        Assert.Equal(2, transactions.Count);

        Assert.Equal("Amazon Purchase", transactions[0].Description);
        Assert.Equal(25.50m, transactions[0].Amount);

        Assert.Equal("Uber Ride", transactions[1].Description);
        Assert.Equal(10.00m, transactions[1].Amount);
    }

    [Fact]
    public async Task ImportCsv_ShouldImportValidRows_AndSkipInvalidRows_WhenFileHasMixedData()
    {
        // Arrange - register and login
        var email = $"import_{Guid.NewGuid():N}@example.com";
        var password = "Password123!";

        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = password
        };

        var loginRequest = new LoginRequestDto
        {
            Email = email,
            Password = password
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginBody);
        Assert.False(string.IsNullOrWhiteSpace(loginBody!.Token));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.Token);

        var meResponse = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var meBody = await meResponse.Content.ReadFromJsonAsync<MeResponseDto>();
        Assert.NotNull(meBody);

        var userId = meBody!.UserId;

        var csv = """
    Date,Description,Amount
    2026-01-10,Amazon Purchase,25.50
    2026-01-11,Invalid Amount,not-a-number
    2026-01-12,Uber Ride,10.00
    """;

        using var multipartContent = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csv));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");

        multipartContent.Add(fileContent, "File", "mixed-import.csv");

        // Act
        var response = await _client.PostAsync("/api/imports", multipartContent);

        // Assert - HTTP response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ImportCsvResponseDto>();
        Assert.NotNull(body);

        Assert.NotEqual(Guid.Empty, body!.ImportJobId);
        Assert.Equal("mixed-import.csv", body.FileName);
        Assert.Equal(2, body.ImportedCount);
        Assert.Equal(1, body.SkippedCount);
        Assert.Single(body.Errors);

        var error = body.Errors[0];
        Assert.Equal(3, error.RowNumber);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));

        // Assert - data persisted in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ExpenseAnalyzerDbContext>();

        var importJob = await dbContext.ImportJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == body.ImportJobId && x.UserId == userId);

        Assert.NotNull(importJob);

        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.ImportJobId == body.ImportJobId && x.UserId == userId)
            .OrderBy(x => x.Date)
            .ToListAsync();

        Assert.Equal(2, transactions.Count);

        Assert.Equal("Amazon Purchase", transactions[0].Description);
        Assert.Equal(25.50m, transactions[0].Amount);

        Assert.Equal("Uber Ride", transactions[1].Description);
        Assert.Equal(10.00m, transactions[1].Amount);
    }

    private class RegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    private class MeResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    private class ImportCsvResponseDto
    {
        public Guid ImportJobId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public DateTime ImportedAtUtc { get; set; }
        public List<ImportCsvRowErrorDto> Errors { get; set; } = [];
    }

    private class ImportCsvRowErrorDto
    {
        public int RowNumber { get; set; }
        public string RawLine { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}