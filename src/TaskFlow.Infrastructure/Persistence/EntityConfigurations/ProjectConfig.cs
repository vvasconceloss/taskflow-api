using TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Infrastructure.Persistence.EntityConfigurations
{
    public class ProjectEntityTypeConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.Property(p => p.Name).HasMaxLength(100);
            builder.Property(p => p.Description).HasMaxLength(2000);
            builder.Property(p => p.WorkspaceId).IsRequired();
            builder.Property(p => p.IsArchived).HasDefaultValue(false);
            builder.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
