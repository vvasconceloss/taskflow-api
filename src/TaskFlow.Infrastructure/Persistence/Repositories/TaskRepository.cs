using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;
using TaskPriority = TaskFlow.Domain.Entities.TaskPriority;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class TaskRepository(ApplicationDbContext dbContext) : ITaskRepository
    {
        public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        public async Task<PagedResult<TaskItem>> GetTasksAsync(
            Guid projectId,
            TaskStatus? status,
            TaskPriority? priority,
            Guid? assigneeId,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = dbContext.TaskItems.Where(t => t.ProjectId == projectId);

            if (status is not null)
            {
                query = query.Where(t => t.Status == status);
            }

            if (priority is not null)
            {
                query = query.Where(t => t.Priority == priority);
            }

            if (assigneeId is not null)
            {
                query = query.Where(t => t.AssigneeUserId == assigneeId);
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await ApplySort(query, sortBy, sortDescending)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PagedResult<TaskItem>(
                items,
                page,
                pageSize,
                totalItems,
                (int)Math.Ceiling(totalItems / (double)pageSize));
        }

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

        private static IQueryable<TaskItem> ApplySort(IQueryable<TaskItem> query, string? sortBy, bool descending) =>
            sortBy switch
            {
                "title" => descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                "status" => descending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                "priority" => descending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                "dueDate" => descending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
                _ => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
            };
    }
}
