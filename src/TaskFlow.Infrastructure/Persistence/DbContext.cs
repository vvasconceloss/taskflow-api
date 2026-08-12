using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Infrastructure.Persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {
    }

}
