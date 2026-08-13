using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using TaskFlow.Api.Logging;

namespace TaskFlow.IntegrationTests;

public class RequestLogEnricherTests
{
    [Fact]
    public void GetUserId_WhenAuthenticated_ReturnsSubClaim()
    {
        var userId = Guid.NewGuid().ToString();
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, userId) }, "test"));

        var result = RequestLogEnricher.GetUserId(principal);

        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_WhenAnonymous_ReturnsNull()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = RequestLogEnricher.GetUserId(principal);

        result.Should().BeNull();
    }
}
