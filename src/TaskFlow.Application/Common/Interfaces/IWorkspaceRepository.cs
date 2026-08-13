using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces
{
    public interface IWorkspaceRepository
    {
        Task<PagedResult<Workspace>> GetWorkspacesForUserAsync(
            Guid userId,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
        Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<WorkspaceMember?> GetMembershipAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken);
        Task<bool> IsMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken);
        Task<bool> IsAdminAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken);
        Task<int> CountAdminsAsync(Guid workspaceId, CancellationToken cancellationToken);
        Task CreateWithOwnerAsync(Workspace workspace, WorkspaceMember owner, CancellationToken cancellationToken);
        Task AddMemberAsync(WorkspaceMember member, CancellationToken cancellationToken);
        Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken);
        Task UpdateMemberAsync(WorkspaceMember member, CancellationToken cancellationToken);
        Task RemoveAsync(Workspace workspace, CancellationToken cancellationToken);
        Task RemoveMemberAsync(WorkspaceMember member, CancellationToken cancellationToken);
    }
}
