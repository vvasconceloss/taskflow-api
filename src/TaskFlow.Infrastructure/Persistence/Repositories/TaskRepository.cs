using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class TaskRepository(ApplicationDbContext dbContext) : ITaskRepository
    {
        public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        public Task<List<TaskItem>> GetTasksByProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
            dbContext.TaskItems
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public Task<Guid?> GetWorkspaceIdByTaskIdAsync(Guid taskId, CancellationToken cancellationToken) =>
            (from task in dbContext.TaskItems
             join project in dbContext.Projects on task.ProjectId equals project.Id
             where task.Id == taskId
             select (Guid?)project.WorkspaceId)
            .FirstOrDefaultAsync(cancellationToken);

        public async Task CreateAsync(TaskItem task, CancellationToken cancellationToken)
        {
            await dbContext.TaskItems.AddAsync(task, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task UpdateAsync(TaskItem task, CancellationToken cancellationToken) =>
            dbContext.SaveChangesAsync(cancellationToken);

        public Task RemoveAsync(TaskItem task, CancellationToken cancellationToken)
        {
            dbContext.TaskItems.Remove(task);
            return dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
