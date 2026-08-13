using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Fixtures;

namespace TaskFlow.IntegrationTests;

public class FullFlowTests : IClassFixture<TaskFlowApiFactory>
{
    private readonly TaskFlowApiFactory _factory;

    public FullFlowTests(TaskFlowApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompleteFlow_RegisterToDoneTask_ShouldSucceed()
    {
        using var anon = _factory.CreateClient();

        var emailA = $"flowA{Guid.NewGuid():N}@example.com";
        var emailB = $"flowB{Guid.NewGuid():N}@example.com";

        var registerA = await anon.PostAsJsonAsync("/auth/register",
            new { email = emailA, password = "StrongPass123" });
        registerA.StatusCode.Should().Be(HttpStatusCode.Created);

        var registerB = await anon.PostAsJsonAsync("/auth/register",
            new { email = emailB, password = "StrongPass123" });
        registerB.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginA = await anon.PostAsJsonAsync("/auth/login", new { email = emailA, password = "StrongPass123" });
        var tokenA = (await loginA.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        var clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var loginB = await anon.PostAsJsonAsync("/auth/login", new { email = emailB, password = "StrongPass123" });
        var tokenB = (await loginB.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        var clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var meB = await clientB.GetAsync("/auth/me");
        var userIdB = (await meB.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var workspace = await clientA.PostAsJsonAsync("/workspaces", new { name = "Acme" });
        workspace.StatusCode.Should().Be(HttpStatusCode.Created);
        var workspaceId = (await workspace.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var invite = await clientA.PostAsJsonAsync($"/workspaces/{workspaceId}/members", new { email = emailB });
        invite.StatusCode.Should().Be(HttpStatusCode.Created);

        var project = await clientA.PostAsJsonAsync($"/workspaces/{workspaceId}/projects", new { name = "Platform" });
        project.StatusCode.Should().Be(HttpStatusCode.Created);
        var projectId = (await project.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var task = await clientA.PostAsJsonAsync($"/projects/{projectId}/tasks",
            new { title = "Set up CI", priority = "High" });
        task.StatusCode.Should().Be(HttpStatusCode.Created);
        var taskId = (await task.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var assign = await clientA.PatchAsJsonAsync($"/tasks/{taskId}/assignee", new { assigneeUserId = userIdB });
        assign.StatusCode.Should().Be(HttpStatusCode.OK);

        var done = await clientA.PatchAsJsonAsync($"/tasks/{taskId}/status", new { status = "Done" });
        done.StatusCode.Should().Be(HttpStatusCode.OK);
        var doneBody = await done.Content.ReadFromJsonAsync<JsonElement>();
        doneBody.GetProperty("status").GetString().Should().Be("Done");
        doneBody.GetProperty("completedAt").GetDateTime().Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
        doneBody.GetProperty("assigneeUserId").GetGuid().Should().Be(userIdB);

        var list = await clientA.GetAsync($"/projects/{projectId}/tasks?status=Done");
        var items = (await list.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("items").EnumerateArray().ToList();
        items.Should().ContainSingle();
        items[0].GetProperty("id").GetGuid().Should().Be(taskId);
    }
}
