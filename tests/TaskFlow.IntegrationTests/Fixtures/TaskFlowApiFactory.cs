using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace TaskFlow.IntegrationTests.Fixtures;

public class TaskFlowApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("taskflow_tests")
        .WithUsername("taskflow")
        .WithPassword("taskflow")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var externalConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = externalConnection ?? _postgres.GetConnectionString(),
                ["Jwt:Secret"] = "test-secret-0123456789abcdef0123456789abcdef",
                ["Jwt:Issuer"] = "TaskFlow.Api",
                ["Jwt:Audience"] = "TaskFlow.Client",
                ["Jwt:ExpiryMinutes"] = "60",
                ["RateLimiting:Login:PermitLimit"] = "1000000"
            });
        });
    }

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") is null)
        {
            await _postgres.StartAsync();
        }

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
