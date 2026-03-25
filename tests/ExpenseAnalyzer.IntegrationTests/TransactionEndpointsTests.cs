using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ExpenseAnalyzer.Domain.Entities;
using ExpenseAnalyzer.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExpenseAnalyzer.IntegrationTests;

public class TransactionEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TransactionEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactions_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        var response = await _client.GetAsync("/api/transactions");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_ShouldReturnOk_WhenTokenIsValid()
    {
        // Arrange
        var email = $"test_{Guid.NewGuid():N}@example.com";
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

        // Act
        var response = await _client.GetAsync("/api/transactions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_ShouldReturnPagedResult_WhenTokenIsValid()
    {
        // Arrange
        var email = $"test_{Guid.NewGuid():N}@example.com";
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

        // Act
        var response = await _client.GetAsync("/api/transactions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResultDto<TransactionSummaryDto>>();
        Assert.NotNull(body);

        Assert.NotNull(body!.Items);
        Assert.Equal(1, body.PageNumber);
        Assert.Equal(20, body.PageSize);
        Assert.True(body.TotalCount >= 0);
        Assert.True(body.TotalPages >= 0);
        Assert.False(body.IsPageOutOfRange);
    }

    [Fact]
    public async Task GetTransactions_ShouldApplyFilterSortingAndPaging_ForAuthenticatedUser()
    {
        // Arrange
        var email = $"test_{Guid.NewGuid():N}@example.com";
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

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ExpenseAnalyzerDbContext>();

            var importJob = new ImportJob
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FileName = "integration-test.csv",
                ImportedAtUtc = DateTime.UtcNow
            };

            dbContext.ImportJobs.Add(importJob);

            dbContext.Transactions.AddRange(
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ImportJobId = importJob.Id,
                    Date = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    Description = "week groceries",
                    Amount = 30m,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ImportJobId = importJob.Id,
                    Date = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc),
                    Description = "week transport",
                    Amount = 80m,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ImportJobId = importJob.Id,
                    Date = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc),
                    Description = "week snacks",
                    Amount = 50m,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ImportJobId = importJob.Id,
                    Date = new DateTime(2026, 1, 13, 0, 0, 0, DateTimeKind.Utc),
                    Description = "rent payment",
                    Amount = 200m,
                    CreatedAtUtc = DateTime.UtcNow
                }
            );

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync(
            "/api/transactions?description=week&sortBy=amount&sortDirection=desc&pageNumber=1&pageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedResultDto<TransactionSummaryDto>>();
        Assert.NotNull(body);

        Assert.Equal(3, body!.TotalCount);
        Assert.Equal(1, body.PageNumber);
        Assert.Equal(2, body.PageSize);
        Assert.Equal(2, body.TotalPages);
        Assert.False(body.IsPageOutOfRange);

        Assert.Equal(2, body.Items.Count);

        Assert.All(body.Items, item => Assert.Contains("week", item.Description, StringComparison.OrdinalIgnoreCase));

        Assert.Equal(80m, body.Items[0].Amount);
        Assert.Equal(50m, body.Items[1].Amount);
    }

    [Fact]
    public async Task GetTransactionById_ShouldReturnNotFound_WhenTransactionDoesNotExist()
    {
        // Arrange
        var email = $"test_{Guid.NewGuid():N}@example.com";
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

        var transactionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/transactions/{transactionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactionById_ShouldReturnNotFound_WhenTransactionBelongsToAnotherUser()
    {
        // Arrange - User A
        var userAEmail = $"usera_{Guid.NewGuid():N}@example.com";
        var password = "Password123!";

        var registerUserARequest = new RegisterRequestDto
        {
            Email = userAEmail,
            Password = password
        };

        var registerUserAResponse = await _client.PostAsJsonAsync("/api/auth/register", registerUserARequest);
        Assert.Equal(HttpStatusCode.Created, registerUserAResponse.StatusCode);

        var loginUserARequest = new LoginRequestDto
        {
            Email = userAEmail,
            Password = password
        };

        var loginUserAResponse = await _client.PostAsJsonAsync("/api/auth/login", loginUserARequest);
        Assert.Equal(HttpStatusCode.OK, loginUserAResponse.StatusCode);

        var loginUserABody = await loginUserAResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginUserABody);
        Assert.False(string.IsNullOrWhiteSpace(loginUserABody!.Token));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginUserABody.Token);

        var meUserAResponse = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meUserAResponse.StatusCode);

        var meUserABody = await meUserAResponse.Content.ReadFromJsonAsync<MeResponseDto>();
        Assert.NotNull(meUserABody);

        var userAId = meUserABody!.UserId;
        var transactionId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ExpenseAnalyzerDbContext>();

            var importJob = new ImportJob
            {
                Id = Guid.NewGuid(),
                UserId = userAId,
                FileName = "ownership-test.csv",
                ImportedAtUtc = DateTime.UtcNow
            };

            dbContext.ImportJobs.Add(importJob);

            dbContext.Transactions.Add(new Transaction
            {
                Id = transactionId,
                UserId = userAId,
                ImportJobId = importJob.Id,
                Date = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Description = "user A private transaction",
                Amount = 99m,
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }

        // Arrange - User B
        var userBEmail = $"userb_{Guid.NewGuid():N}@example.com";

        var registerUserBRequest = new RegisterRequestDto
        {
            Email = userBEmail,
            Password = password
        };

        var registerUserBResponse = await _client.PostAsJsonAsync("/api/auth/register", registerUserBRequest);
        Assert.Equal(HttpStatusCode.Created, registerUserBResponse.StatusCode);

        var loginUserBRequest = new LoginRequestDto
        {
            Email = userBEmail,
            Password = password
        };

        var loginUserBResponse = await _client.PostAsJsonAsync("/api/auth/login", loginUserBRequest);
        Assert.Equal(HttpStatusCode.OK, loginUserBResponse.StatusCode);

        var loginUserBBody = await loginUserBResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginUserBBody);
        Assert.False(string.IsNullOrWhiteSpace(loginUserBBody!.Token));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginUserBBody.Token);

        // Act
        var response = await _client.GetAsync($"/api/transactions/{transactionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

    private class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public bool IsPageOutOfRange { get; set; }
        public string? Message { get; set; }
    }

    private class TransactionSummaryDto
    {
        public Guid TransactionId { get; set; }
        public Guid ImportJobId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    private class MeResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}

