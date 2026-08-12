using TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Infrastructure.Persistence.EntityConfigurations
{
    public class WorkspaceEntityTypeConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.Property(w => w.Name).HasMaxLength(100);
            builder.Property(w => w.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(w => w.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(w => w.Projects)
                .WithOne()
                .HasForeignKey(p => p.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
