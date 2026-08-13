using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Fixtures;

namespace TaskFlow.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<TaskFlowApiFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(TaskFlowApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithNewEmail_ShouldReturn201()
    {
        var response = await _client.PostAsJsonAsync("/auth/register",
            new { email = $"user{Guid.NewGuid():N}@example.com", password = "StrongPass123" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409()
    {
        var body = new { email = $"dup{Guid.NewGuid():N}@example.com", password = "StrongPass123" };

        var first = await _client.PostAsJsonAsync("/auth/register", body);
        var second = await _client.PostAsJsonAsync("/auth/register", body);

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var email = $"login{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/auth/register", new { email, password = "StrongPass123" });

        var response = await _client.PostAsJsonAsync("/auth/login", new { email, password = "StrongPass123" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("expiresAt").GetDateTime().Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401()
    {
        var response = await _client.PostAsJsonAsync("/auth/login",
            new { email = "ghost@example.com", password = "WrongPass123" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ShouldReturnUser()
    {
        var email = $"me{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/auth/register", new { email, password = "StrongPass123" });
        var login = await _client.PostAsJsonAsync("/auth/login", new { email, password = "StrongPass123" });
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();

        var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("email").GetString().Should().Be(email);
    }

    [Fact]
    public async Task GetMe_WithoutToken_ShouldReturn401()
    {
        var response = await _client.GetAsync("/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
