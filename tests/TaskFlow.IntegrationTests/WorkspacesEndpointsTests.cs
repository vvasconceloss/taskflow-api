using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Fixtures;

namespace TaskFlow.IntegrationTests;

public class WorkspacesEndpointsTests : IClassFixture<TaskFlowApiFactory>
{
    private readonly TaskFlowApiFactory _factory;

    public WorkspacesEndpointsTests(TaskFlowApiFactory factory)
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

    private static async Task<Guid> CreateWorkspaceAsync(HttpClient client, string name = "Acme")
    {
        var response = await client.PostAsJsonAsync("/workspaces", new { name });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task Create_And_List_ShouldReturnOwnedWorkspace()
    {
        var (client, _) = await CreateUserAsync("owner");

        var workspaceId = await CreateWorkspaceAsync(client, "Acme");

        var list = await client.GetAsync("/workspaces");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await list.Content.ReadFromJsonAsync<List<JsonElement>>();
        items.Should().Contain(item => item.GetProperty("id").GetGuid() == workspaceId);
    }

    [Fact]
    public async Task NonMember_Cannot_See_OtherUsersWorkspace()
    {
        var (owner, _) = await CreateUserAsync("owner");
        var (intruder, _) = await CreateUserAsync("intruder");
        var workspaceId = await CreateWorkspaceAsync(owner);

        var detail = await intruder.GetAsync($"/workspaces/{workspaceId}");

        detail.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var list = await intruder.GetAsync("/workspaces");
        var items = await list.Content.ReadFromJsonAsync<List<JsonElement>>();
        items.Should().NotContain(item => item.GetProperty("id").GetGuid() == workspaceId);
    }

    [Fact]
    public async Task Admin_Can_Update_Workspace()
    {
        var (admin, _) = await CreateUserAsync("admin");
        var workspaceId = await CreateWorkspaceAsync(admin, "Acme");

        var response = await admin.PatchAsJsonAsync($"/workspaces/{workspaceId}", new { name = "Acme 2" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Acme 2");
    }

    [Fact]
    public async Task Member_WithoutAdminRole_Cannot_Update_Workspace()
    {
        var (admin, _) = await CreateUserAsync("admin");
        var (member, memberEmail) = await CreateUserAsync("member");
        var workspaceId = await CreateWorkspaceAsync(admin);
        await admin.PostAsJsonAsync($"/workspaces/{workspaceId}/members", new { email = memberEmail });

        var response = await member.PatchAsJsonAsync($"/workspaces/{workspaceId}", new { name = "Hijack" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddMember_ByEmail_ShouldReturn201()
    {
        var (admin, _) = await CreateUserAsync("admin");
        var (_, newMemberEmail) = await CreateUserAsync("newmember");
        var workspaceId = await CreateWorkspaceAsync(admin);

        var response = await admin.PostAsJsonAsync($"/workspaces/{workspaceId}/members", new { email = newMemberEmail });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddMember_WithUnknownEmail_ShouldReturn404()
    {
        var (admin, _) = await CreateUserAsync("admin");
        var workspaceId = await CreateWorkspaceAsync(admin);

        var response = await admin.PostAsJsonAsync($"/workspaces/{workspaceId}/members",
            new { email = "ghost@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddMember_WhenAlreadyMember_ShouldReturn409()
    {
        var (admin, _) = await CreateUserAsync("admin");
        var (_, memberEmail) = await CreateUserAsync("dup");
        var workspaceId = await CreateWorkspaceAsync(admin);
        await admin.PostAsJsonAsync($"/workspaces/{workspaceId}/members", new { email = memberEmail });

        var second = await admin.PostAsJsonAsync($"/workspaces/{workspaceId}/members", new { email = memberEmail });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Remove_LastAdmin_ShouldReturn409()
    {
        var (admin, adminEmail) = await CreateUserAsync("soloadmin");
        var workspaceId = await CreateWorkspaceAsync(admin);
        var me = await admin.GetAsync("/auth/me");
        var adminId = (await me.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        _ = adminEmail;

        var response = await admin.DeleteAsync($"/workspaces/{workspaceId}/members/{adminId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Demote_LastAdmin_ShouldReturn409()
    {
        var (admin, _) = await CreateUserAsync("demote");
        var workspaceId = await CreateWorkspaceAsync(admin);
        var me = await admin.GetAsync("/auth/me");
        var adminId = (await me.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var response = await admin.PatchAsJsonAsync($"/workspaces/{workspaceId}/members/{adminId}",
            new { role = "Member" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Admin_Can_Remove_NonAdminMember()
    {
        var (admin, _) = await CreateUserAsync("admin");
        var (member, memberEmail) = await CreateUserAsync("member");
        var workspaceId = await CreateWorkspaceAsync(admin);
        await admin.PostAsJsonAsync($"/workspaces/{workspaceId}/members", new { email = memberEmail });
        var memberMe = await member.GetAsync("/auth/me");
        var memberId = (await memberMe.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var response = await admin.DeleteAsync($"/workspaces/{workspaceId}/members/{memberId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Admin_Can_Delete_Workspace()
    {
        var (admin, _) = await CreateUserAsync("deleter");
        var workspaceId = await CreateWorkspaceAsync(admin);

        var response = await admin.DeleteAsync($"/workspaces/{workspaceId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
