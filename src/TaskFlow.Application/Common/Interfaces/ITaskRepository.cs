using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;
using TaskPriority = TaskFlow.Domain.Entities.TaskPriority;

namespace TaskFlow.Application.Common.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<PagedResult<TaskItem>> GetTasksAsync(
            Guid projectId,
            TaskStatus? status,
            TaskPriority? priority,
            Guid? assigneeId,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
        Task<Guid?> GetWorkspaceIdByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
        Task CreateAsync(TaskItem task, CancellationToken cancellationToken);
        Task UpdateAsync(TaskItem task, CancellationToken cancellationToken);
        Task RemoveAsync(TaskItem task, CancellationToken cancellationToken);
    }
}
