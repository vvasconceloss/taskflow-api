using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.IntegrationTests.Fixtures;

namespace TaskFlow.IntegrationTests;

public class ProjectsEndpointsTests : IClassFixture<TaskFlowApiFactory>
{
    private readonly TaskFlowApiFactory _factory;

    public ProjectsEndpointsTests(TaskFlowApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient Client, string Email)> CreateUserAsync(string prefix)
    {
        using var anon = _factory.CreateClient();
        var email = $"{prefix}{Guid.NewGuid():N}@example.com";
        await anon.PostAsJsonAsync("/auth/register", new { email, password = "StrongPass123" });
        var login = await anon.PostAsJsonAsync("/auth/login", new { email, password = "StrongPass123" });
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, email);
    }

    private static async Task<Guid> CreateWorkspaceAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/workspaces", new { name = $"WS{Guid.NewGuid():N}" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateProjectAsync(HttpClient client, Guid workspaceId, string name = "Project")
    {
        var response = await client.PostAsJsonAsync($"/workspaces/{workspaceId}/projects", new { name });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task Create_And_List_ShouldReturnProject()
    {
        var (client, _) = await CreateUserAsync("powner");
        var workspaceId = await CreateWorkspaceAsync(client);

        var projectId = await CreateProjectAsync(client, workspaceId, "Backend");

        var list = await client.GetAsync($"/workspaces/{workspaceId}/projects");
        var items = await list.Content.ReadFromJsonAsync<List<JsonElement>>();
        items.Should().Contain(item => item.GetProperty("id").GetGuid() == projectId);
    }

    [Fact]
    public async Task Archive_RemovesFromListing_ButKeepsAccessibleById()
    {
        var (client, _) = await CreateUserAsync("archiver");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId, "Old");

        var archive = await client.PostAsJsonAsync($"/projects/{projectId}/archive", new { });
        archive.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await client.GetAsync($"/workspaces/{workspaceId}/projects");
        var items = await list.Content.ReadFromJsonAsync<List<JsonElement>>();
        items.Should().NotContain(item => item.GetProperty("id").GetGuid() == projectId);

        var detail = await client.GetAsync($"/projects/{projectId}");
        detail.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await detail.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isArchived").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task NonMember_Cannot_Access_Project()
    {
        var (owner, _) = await CreateUserAsync("powner");
        var (intruder, _) = await CreateUserAsync("pintruder");
        var workspaceId = await CreateWorkspaceAsync(owner);
        var projectId = await CreateProjectAsync(owner, workspaceId);

        var create = await intruder.PostAsJsonAsync($"/workspaces/{workspaceId}/projects", new { name = "Sneaky" });
        create.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var detail = await intruder.GetAsync($"/projects/{projectId}");
        detail.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Member_Cannot_Access_Project_Of_Another_Workspace()
    {
        var (ownerA, _) = await CreateUserAsync("aowner");
        var (ownerB, memberBEmail) = await CreateUserAsync("bowner");
        var workspaceA = await CreateWorkspaceAsync(ownerA);
        var projectA = await CreateProjectAsync(ownerA, workspaceA, "Confidential");

        var workspaceB = await CreateWorkspaceAsync(ownerB);
        await ownerB.PostAsJsonAsync($"/workspaces/{workspaceB}/members", new { email = memberBEmail });

        var detail = await ownerB.GetAsync($"/projects/{projectA}");

        detail.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_Project_ShouldReturnUpdatedFields()
    {
        var (client, _) = await CreateUserAsync("pupdater");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId, "Before");

        var response = await client.PatchAsJsonAsync($"/projects/{projectId}",
            new { name = "After", description = "Renamed" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("After");
        body.GetProperty("description").GetString().Should().Be("Renamed");
    }

    [Fact]
    public async Task Delete_Project_WithoutTasks_ShouldReturn204()
    {
        var (client, _) = await CreateUserAsync("pdeleter");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId);

        var response = await client.DeleteAsync($"/projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_Project_WithTasks_ShouldReturn409()
    {
        var (client, _) = await CreateUserAsync("ptasks");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.TaskItems.Add(new TaskItem { Title = "Blocking task", ProjectId = projectId });
        await db.SaveChangesAsync();

        var response = await client.DeleteAsync($"/projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
