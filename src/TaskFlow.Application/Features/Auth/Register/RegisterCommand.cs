using MediatR;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Auth.Register
{
    public record RegisterCommand(string? Name, string Email, string Password) : IRequest<User>;
}
