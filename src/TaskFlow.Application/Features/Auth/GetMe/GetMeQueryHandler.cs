using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Auth.GetMe
{
    public class GetMeQueryHandler(IUserRepository users, ICurrentUserService currentUser)
        : IRequestHandler<GetMeQuery, User>
    {
        public async Task<User> Handle(GetMeQuery request, CancellationToken cancellationToken)
        {
            return await users.GetByIdAsync(currentUser.UserId, cancellationToken)
                ?? throw new NotFoundException("User not found.");
        }
    }
}
