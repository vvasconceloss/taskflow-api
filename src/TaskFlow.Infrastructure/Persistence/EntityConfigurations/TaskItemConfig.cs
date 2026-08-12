using TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Infrastructure.Persistence.EntityConfigurations
{
    public class TaskItemEntityTypeConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.Property(t => t.Title).HasMaxLength(200);
            builder.Property(t => t.Description).HasMaxLength(2000);
            builder.Property(t => t.Status).HasDefaultValue(TaskFlow.Domain.Entities.TaskStatus.Todo);
            builder.Property(t => t.Priority).HasDefaultValue(TaskPriority.Low);
            builder.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne<Project>()
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.AssigneeUserId);
        }
    }
}
