using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            await dbContext.Users.AddAsync(user, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
