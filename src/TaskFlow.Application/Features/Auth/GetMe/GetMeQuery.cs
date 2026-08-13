using MediatR;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Auth.GetMe
{
    public record GetMeQuery : IRequest<User>;
}
