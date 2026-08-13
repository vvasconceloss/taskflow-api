using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Auth.Login
{
    public class LoginCommandHandler(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService)
        : IRequestHandler<LoginCommand, TokenResult>
    {
        public async Task<TokenResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await users.GetByEmailAsync(request.Email, cancellationToken);

            if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            return tokenService.CreateToken(user);
        }
    }
}
