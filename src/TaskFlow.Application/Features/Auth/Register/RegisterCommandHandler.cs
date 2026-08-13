using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Auth.Register
{
    public class RegisterCommandHandler(IUserRepository users, IPasswordHasher passwordHasher)
        : IRequestHandler<RegisterCommand, User>
    {
        public async Task<User> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (await users.GetByEmailAsync(request.Email, cancellationToken) is not null)
            {
                throw new ConflictException($"A user with the email '{request.Email}' already exists.");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHasher.Hash(request.Password),
                Name = request.Name
            };

            await users.AddAsync(user, cancellationToken);

            return user;
        }
    }
}
