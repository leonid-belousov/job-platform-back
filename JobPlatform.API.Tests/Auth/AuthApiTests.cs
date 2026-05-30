using System.Net;
using System.Net.Http.Json;
using JobPlatform.API.Tests.Infrastructure;

namespace JobPlatform.API.Tests.Auth;


public sealed class AuthApiTests : ApiTestBase
{
    public AuthApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidCandidate_ReturnsAccessAndRefreshTokens()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "candidate@example.com",
            password = "Password123",
            roleCode = "candidate"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadAsAsync<AuthPayload>();
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload.RefreshToken));
        Assert.Equal("candidate@example.com", payload.Email);
    }

    [Fact]
    public async Task Register_WithAdminRole_ReturnsBadRequest()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "admin-public@example.com",
            password = "Password123",
            roleCode = "admin"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithRegisteredUser_ReturnsTokens()
    {
        var client = Factory.CreateClient();

        var signUpResponse = await client.PostAsJsonAsync("/api/auth/sign-up", new
        {
            email = "employer@example.com",
            password = "Password123",
            roleCode = "employer"
        });

        var signUpBody = await signUpResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, signUpResponse.StatusCode);

        var response = await client.PostAsJsonAsync("/api/auth/sign-in", new
        {
            email = "employer@example.com",
            password = "Password123"
        });

        var signInBody = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<AuthPayload>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload.RefreshToken));
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed record AuthPayload(string AccessToken, string RefreshToken, Guid UserId, string Email);
}
