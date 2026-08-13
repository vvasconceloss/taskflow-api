using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Api.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public Guid UserId => Guid.Parse(
            httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? Guid.Empty.ToString());
    }
}
