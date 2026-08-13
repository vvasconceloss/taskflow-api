using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskFlow.Api.Logging
{
    public static class RequestLogEnricher
    {
        public static string? GetUserId(ClaimsPrincipal user) =>
            user.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}
