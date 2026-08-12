using TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Infrastructure.Persistence.EntityConfigurations
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.Name).HasMaxLength(100);
            builder.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
