using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Fixtures;

namespace TaskFlow.IntegrationTests;

public class ValidationTests : IClassFixture<TaskFlowApiFactory>
{
    private readonly TaskFlowApiFactory _factory;

    public ValidationTests(TaskFlowApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string prefix)
    {
        using var anon = _factory.CreateClient();
        var email = $"{prefix}{Guid.NewGuid():N}@example.com";
        await anon.PostAsJsonAsync("/auth/register", new { email, password = "StrongPass123" });
        var login = await anon.PostAsJsonAsync("/auth/login", new { email, password = "StrongPass123" });
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/auth/register",
            new { email = $"weak{Guid.NewGuid():N}@example.com", password = "onlyletters" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("type").GetString().Should().Be("ValidationError");
        body.GetProperty("errors").GetProperty("Password").EnumerateArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/auth/register",
            new { email = "not-an-email", password = "StrongPass123" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateWorkspace_WithEmptyName_ShouldReturn400()
    {
        var client = await CreateAuthenticatedClientAsync("emptyws");

        var response = await client.PostAsJsonAsync("/workspaces", new { name = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errors").GetProperty("Name").EnumerateArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTask_WithPastDueDate_ShouldReturn400()
    {
        var client = await CreateAuthenticatedClientAsync("pastdue");
        var workspace = await client.PostAsJsonAsync("/workspaces", new { name = "WS" });
        var workspaceId = (await workspace.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var project = await client.PostAsJsonAsync($"/workspaces/{workspaceId}/projects", new { name = "Project" });
        var projectId = (await project.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var response = await client.PostAsJsonAsync($"/projects/{projectId}/tasks",
            new { title = "Late", dueDate = DateTime.UtcNow.AddDays(-1).ToString("O") });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
