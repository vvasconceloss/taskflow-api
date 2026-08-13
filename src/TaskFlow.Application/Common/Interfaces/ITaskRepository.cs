using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<TaskItem>> GetTasksByProjectAsync(Guid projectId, CancellationToken cancellationToken);
        Task<Guid?> GetWorkspaceIdByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
        Task CreateAsync(TaskItem task, CancellationToken cancellationToken);
        Task UpdateAsync(TaskItem task, CancellationToken cancellationToken);
        Task RemoveAsync(TaskItem task, CancellationToken cancellationToken);
    }
}
