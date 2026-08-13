using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<PagedResult<Project>> GetProjectsAsync(
            Guid workspaceId,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
        Task<Guid?> GetWorkspaceIdByProjectIdAsync(Guid projectId, CancellationToken cancellationToken);
        Task<bool> HasTasksAsync(Guid projectId, CancellationToken cancellationToken);
        Task CreateAsync(Project project, CancellationToken cancellationToken);
        Task UpdateAsync(Project project, CancellationToken cancellationToken);
        Task RemoveAsync(Project project, CancellationToken cancellationToken);
    }
}
