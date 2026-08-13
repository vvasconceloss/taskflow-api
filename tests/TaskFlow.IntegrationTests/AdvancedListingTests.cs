using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlow.IntegrationTests.Fixtures;

namespace TaskFlow.IntegrationTests;

public class AdvancedListingTests : IClassFixture<TaskFlowApiFactory>
{
    private readonly TaskFlowApiFactory _factory;

    public AdvancedListingTests(TaskFlowApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateUserAsync(string prefix)
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

    private static async Task<Guid> CreateWorkspaceAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/workspaces", new { name = $"WS{Guid.NewGuid():N}" });
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateProjectAsync(HttpClient client, Guid workspaceId, string name)
    {
        var response = await client.PostAsJsonAsync($"/workspaces/{workspaceId}/projects", new { name });
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private static async Task<List<JsonElement>> GetItemsAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<JsonElement>();
        return paged.GetProperty("items").EnumerateArray().ToList();
    }

    [Fact]
    public async Task Tasks_Filter_ByStatus()
    {
        var client = await CreateUserAsync("tfilter");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId, "Project");

        var doneResponse = await client.PostAsJsonAsync($"/projects/{projectId}/tasks", new { title = "Done task" });
        var doneId = (await doneResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        await client.PostAsJsonAsync($"/projects/{projectId}/tasks", new { title = "Todo task" });
        await client.PatchAsJsonAsync($"/tasks/{doneId}/status", new { status = "Done" });

        var items = await GetItemsAsync(client, $"/projects/{projectId}/tasks?status=Done");

        items.Should().ContainSingle();
        items[0].GetProperty("id").GetGuid().Should().Be(doneId);
    }

    [Fact]
    public async Task Tasks_CombinedFilters_PriorityAndStatus()
    {
        var client = await CreateUserAsync("tcomb");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId, "Project");

        var high = await client.PostAsJsonAsync($"/projects/{projectId}/tasks", new { title = "High", priority = "High" });
        var highId = (await high.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        await client.PostAsJsonAsync($"/projects/{projectId}/tasks", new { title = "Low", priority = "Low" });
        await client.PostAsJsonAsync($"/projects/{projectId}/tasks", new { title = "High todo", priority = "High" });
        await client.PatchAsJsonAsync($"/tasks/{highId}/status", new { status = "Done" });

        var items = await GetItemsAsync(client, $"/projects/{projectId}/tasks?priority=High&status=Done");

        items.Should().ContainSingle();
        items[0].GetProperty("id").GetGuid().Should().Be(highId);
    }

    [Fact]
    public async Task Tasks_Sort_ByDueDate_AscAndDesc()
    {
        var client = await CreateUserAsync("tsort");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId, "Project");

        var later = await client.PostAsJsonAsync($"/projects/{projectId}/tasks",
            new { title = "Later", dueDate = DateTime.UtcNow.AddDays(5).ToString("O") });
        var laterId = (await later.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var sooner = await client.PostAsJsonAsync($"/projects/{projectId}/tasks",
            new { title = "Sooner", dueDate = DateTime.UtcNow.AddDays(1).ToString("O") });
        var soonerId = (await sooner.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var asc = await GetItemsAsync(client, $"/projects/{projectId}/tasks?sortBy=dueDate&sortDir=asc");
        asc[0].GetProperty("id").GetGuid().Should().Be(soonerId);
        asc[1].GetProperty("id").GetGuid().Should().Be(laterId);

        var desc = await GetItemsAsync(client, $"/projects/{projectId}/tasks?sortBy=dueDate&sortDir=desc");
        desc[0].GetProperty("id").GetGuid().Should().Be(laterId);
        desc[1].GetProperty("id").GetGuid().Should().Be(soonerId);
    }

    [Fact]
    public async Task Tasks_PageBeyondLast_ReturnsEmptyItems()
    {
        var client = await CreateUserAsync("tpage");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId, "Project");
        await client.PostAsJsonAsync($"/projects/{projectId}/tasks", new { title = "One" });

        var items = await GetItemsAsync(client, $"/projects/{projectId}/tasks?page=2&pageSize=1");

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task Tasks_InvalidSortField_Returns400()
    {
        var client = await CreateUserAsync("tsortbad");
        var workspaceId = await CreateWorkspaceAsync(client);
        var projectId = await CreateProjectAsync(client, workspaceId, "Project");

        var response = await client.GetAsync($"/projects/{projectId}/tasks?sortBy=description");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Projects_Sorted_ByNameAscAndDesc()
    {
        var client = await CreateUserAsync("psort");
        var workspaceId = await CreateWorkspaceAsync(client);
        await client.PostAsJsonAsync($"/workspaces/{workspaceId}/projects", new { name = "Zeta" });
        await client.PostAsJsonAsync($"/workspaces/{workspaceId}/projects", new { name = "Alpha" });

        var asc = await GetItemsAsync(client, $"/workspaces/{workspaceId}/projects?sortBy=name&sortDir=asc");
        asc[0].GetProperty("name").GetString().Should().Be("Alpha");
        asc[1].GetProperty("name").GetString().Should().Be("Zeta");

        var desc = await GetItemsAsync(client, $"/workspaces/{workspaceId}/projects?sortBy=name&sortDir=desc");
        desc[0].GetProperty("name").GetString().Should().Be("Zeta");
    }

    [Fact]
    public async Task Workspaces_Paginated_AndSorted()
    {
        var client = await CreateUserAsync("wsort");
        await client.PostAsJsonAsync("/workspaces", new { name = "Beta" });
        await client.PostAsJsonAsync("/workspaces", new { name = "Alpha" });
        await client.PostAsJsonAsync("/workspaces", new { name = "Gamma" });

        var asc = await GetItemsAsync(client, "/workspaces?sortBy=name&sortDir=asc&pageSize=2");
        asc.Should().HaveCount(2);
        asc[0].GetProperty("name").GetString().Should().Be("Alpha");
        asc[1].GetProperty("name").GetString().Should().Be("Beta");

        var page2 = await GetItemsAsync(client, "/workspaces?sortBy=name&sortDir=asc&page=2&pageSize=2");
        page2.Should().ContainSingle();
        page2[0].GetProperty("name").GetString().Should().Be("Gamma");
    }
}
