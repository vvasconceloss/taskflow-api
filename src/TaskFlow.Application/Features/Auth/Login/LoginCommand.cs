using MediatR;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Auth.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<TokenResult>;
}
