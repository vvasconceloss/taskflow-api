using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces
{
    public record TokenResult(string Token, DateTime ExpiresAt);

    public interface ITokenService
    {
        TokenResult CreateToken(User user);
    }
}
