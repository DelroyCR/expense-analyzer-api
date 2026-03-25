using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace ExpenseAnalyzer.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnCreated()
    {
        var email = $"test_{Guid.NewGuid():N}@example.com";

        var request = new RegisterRequestDto
        {
            Email = email,
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(email, body);
    }

    [Fact]
    public async Task Me_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_Then_Me_ShouldReturnAuthenticatedUser()
    {
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
        Assert.Equal(email, meBody!.Email);
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
}