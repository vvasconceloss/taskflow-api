using TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Infrastructure.Persistence.EntityConfigurations
{
    public class WorkspaceMemberEntityTypeConfiguration : IEntityTypeConfiguration<WorkspaceMember>
    {
        public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
        {
            builder.Property(wm => wm.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasIndex(wm => new { wm.UserId, wm.WorkspaceId }).IsUnique();

            builder.HasOne<User>()
                .WithMany(u => u.Memberships)
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Workspace>()
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
