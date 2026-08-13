using FluentAssertions;
using Npgsql;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.IntegrationTests;

public class PostgresConnectionStringParserTests
{
    [Fact]
    public void ToNpgsqlFormat_WhenNull_ReturnsNull()
    {
        PostgresConnectionStringParser.ToNpgsqlFormat(null).Should().BeNull();
    }

    [Fact]
    public void ToNpgsqlFormat_WhenKeywordFormat_ReturnsUnchanged()
    {
        const string cs = "Host=localhost;Port=5432;Database=db;Username=user;Password=pass";

        var result = PostgresConnectionStringParser.ToNpgsqlFormat(cs);

        result.Should().Be(cs);
    }

    [Fact]
    public void ToNpgsqlFormat_WhenPostgresUri_ConvertsToKeywords()
    {
        var result = PostgresConnectionStringParser.ToNpgsqlFormat(
            "postgresql://taskflow_user:PASS@dpg-xxxx-a:5432/taskflow_db");

        var parsed = new NpgsqlConnectionStringBuilder(result);
        parsed.Host.Should().Be("dpg-xxxx-a");
        parsed.Port.Should().Be(5432);
        parsed.Database.Should().Be("taskflow_db");
        parsed.Username.Should().Be("taskflow_user");
        parsed.Password.Should().Be("PASS");
    }

    [Fact]
    public void ToNpgsqlFormat_WhenPostgresUriWithoutPort_UsesDefaultPort()
    {
        var result = PostgresConnectionStringParser.ToNpgsqlFormat(
            "postgresql://taskflow_user:PASS@dpg-xxxx-a/taskflow_db");

        var parsed = new NpgsqlConnectionStringBuilder(result);
        parsed.Host.Should().Be("dpg-xxxx-a");
        parsed.Port.Should().Be(5432);
        parsed.Database.Should().Be("taskflow_db");
        parsed.Username.Should().Be("taskflow_user");
        parsed.Password.Should().Be("PASS");
    }

    [Fact]
    public void ToNpgsqlFormat_WhenPostgresUriWithEncodedPassword_Decodes()
    {
        var result = PostgresConnectionStringParser.ToNpgsqlFormat(
            "postgresql://user:p%40ss@host:5432/db");

        var parsed = new NpgsqlConnectionStringBuilder(result);
        parsed.Password.Should().Be("p@ss");
    }

    [Fact]
    public void ToNpgsqlFormat_WhenPostgresUriWithQueryParams_AppliesThem()
    {
        var result = PostgresConnectionStringParser.ToNpgsqlFormat(
            "postgresql://user:pass@host:5432/db?sslmode=require");

        var parsed = new NpgsqlConnectionStringBuilder(result);
        parsed.SslMode.Should().Be(SslMode.Require);
    }
}
