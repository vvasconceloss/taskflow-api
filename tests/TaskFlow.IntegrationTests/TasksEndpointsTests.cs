using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Fixtures;

namespace TaskFlow.IntegrationTests;

public class TasksEndpointsTests : IClassFixture<TaskFlowApiFactory>
{
    private readonly TaskFlowApiFactory _factory;

    public TasksEndpointsTests(TaskFlowApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient Client, string Email, Guid UserId)> CreateUserAsync(string prefix)
    {
        using var anon = _factory.CreateClient();
        var email = $"{prefix}{Guid.NewGuid():N}@example.com";
        await anon.PostAsJsonAsync("/auth/register", new { email, password = "StrongPass123" });
        var login = await anon.PostAsJsonAsync("/auth/login", new { email, password = "StrongPass123" });
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var me = await client.GetAsync("/auth/me");
        var userId = (await me.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        return (client, email, userId);
    }

    private static async Task<Guid> CreateWorkspaceAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/workspaces", new { name = $"WS{Guid.NewGuid():N}" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateProjectAsync(HttpClient client, Guid workspaceId)
    {
        var response = await client.PostAsJsonAsync($"/workspaces/{workspaceId}/projects", new { name = "Project" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateTaskAsync(HttpClient client, Guid projectId, string title = "Task")
    {
        var response = await client.PostAsJsonAsync($"/projects/{projectId}/tasks", new { title });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task Create_And_List_ShouldReturnTask()
    {
        var (client, _, _) = await CreateUserAsync("towner");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId);

        var taskId = await CreateTaskAsync(client, projectId, "Setup CI");

        var list = await client.GetAsync($"/projects/{projectId}/tasks");
        var paged = await list.Content.ReadFromJsonAsync<JsonElement>();
        var items = paged.GetProperty("items").EnumerateArray().ToList();
        items.Should().Contain(item => item.GetProperty("id").GetGuid() == taskId);
    }

    [Fact]
    public async Task Get_And_Update_ShouldReturnUpdatedFields()
    {
        var (client, _, _) = await CreateUserAsync("tupdate");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId);
        var taskId = await CreateTaskAsync(client, projectId);

        var get = await client.GetAsync($"/tasks/{taskId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var update = await client.PatchAsJsonAsync($"/tasks/{taskId}",
            new { title = "Renamed", description = "Detail", priority = "High" });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await update.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("title").GetString().Should().Be("Renamed");
        body.GetProperty("priority").GetString().Should().Be("High");
    }

    [Fact]
    public async Task SetStatusDone_ShouldFillCompletedAt_AndBackToTodo_ShouldClearIt()
    {
        var (client, _, _) = await CreateUserAsync("tdone");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId);
        var taskId = await CreateTaskAsync(client, projectId);

        var done = await client.PatchAsJsonAsync($"/tasks/{taskId}/status", new { status = "Done" });
        done.StatusCode.Should().Be(HttpStatusCode.OK);
        var doneBody = await done.Content.ReadFromJsonAsync<JsonElement>();
        doneBody.GetProperty("completedAt").GetDateTime().Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));

        var todo = await client.PatchAsJsonAsync($"/tasks/{taskId}/status", new { status = "Todo" });
        var todoBody = await todo.Content.ReadFromJsonAsync<JsonElement>();
        todoBody.GetProperty("completedAt").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Assign_ToMember_ShouldSucceed()
    {
        var (admin, _, _) = await CreateUserAsync("aassign");
        var (member, memberEmail, memberId) = await CreateUserAsync("massign");
        var workspaceId = await CreateWorkspaceAsync(admin);
        var projectId = await CreateProjectAsync(admin, workspaceId);
        var taskId = await CreateTaskAsync(admin, projectId);
        await admin.PostAsJsonAsync($"/workspaces/{workspaceId}/members", new { email = memberEmail });

        var response = await admin.PatchAsJsonAsync($"/tasks/{taskId}/assignee", new { assigneeUserId = memberId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("assigneeUserId").GetGuid().Should().Be(memberId);
    }

    [Fact]
    public async Task Assign_ToNonMember_ShouldFail()
    {
        var (admin, _, _) = await CreateUserAsync("nassign");
        var (outsider, _, outsiderId) = await CreateUserAsync("oassign");
        var workspaceId = await CreateWorkspaceAsync(admin);
        var projectId = await CreateProjectAsync(admin, workspaceId);
        var taskId = await CreateTaskAsync(admin, projectId);
        _ = outsider;

        var response = await admin.PatchAsJsonAsync($"/tasks/{taskId}/assignee", new { assigneeUserId = outsiderId });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_ShouldReturn204()
    {
        var (client, _, _) = await CreateUserAsync("tdelete");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId);
        var taskId = await CreateTaskAsync(client, projectId);

        var response = await client.DeleteAsync($"/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task NonMember_Cannot_Access_Task()
    {
        var (owner, _, _) = await CreateUserAsync("town");
        var (intruder, _, _) = await CreateUserAsync("tintruder");
        var workspaceId = await CreateWorkspaceAsync(owner);
        var projectId = await CreateProjectAsync(owner, workspaceId);
        var taskId = await CreateTaskAsync(owner, projectId);

        var list = await intruder.GetAsync($"/projects/{projectId}/tasks");
        list.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var detail = await intruder.GetAsync($"/tasks/{taskId}");
        detail.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
