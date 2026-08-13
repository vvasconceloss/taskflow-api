using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class WorkspaceRepository(ApplicationDbContext dbContext) : IWorkspaceRepository
    {
        public Task<List<Workspace>> GetWorkspacesForUserAsync(Guid userId, CancellationToken cancellationToken) =>
            (from membership in dbContext.WorkspaceMembers
             join workspace in dbContext.Workspaces on membership.WorkspaceId equals workspace.Id
             where membership.UserId == userId
             select workspace)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Workspaces.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        public Task<WorkspaceMember?> GetMembershipAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken) =>
            dbContext.WorkspaceMembers.FirstOrDefaultAsync(
                wm => wm.WorkspaceId == workspaceId && wm.UserId == userId, cancellationToken);

        public Task<bool> IsMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken) =>
            dbContext.WorkspaceMembers.AnyAsync(
                wm => wm.WorkspaceId == workspaceId && wm.UserId == userId, cancellationToken);

        public Task<bool> IsAdminAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken) =>
            dbContext.WorkspaceMembers.AnyAsync(
                wm => wm.WorkspaceId == workspaceId && wm.UserId == userId && wm.Role == WorkspaceRole.Admin,
                cancellationToken);

        public Task<int> CountAdminsAsync(Guid workspaceId, CancellationToken cancellationToken) =>
            dbContext.WorkspaceMembers.CountAsync(
                wm => wm.WorkspaceId == workspaceId && wm.Role == WorkspaceRole.Admin, cancellationToken);

        public async Task CreateWithOwnerAsync(Workspace workspace, WorkspaceMember owner, CancellationToken cancellationToken)
        {
            await dbContext.Workspaces.AddAsync(workspace, cancellationToken);
            await dbContext.WorkspaceMembers.AddAsync(owner, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddMemberAsync(WorkspaceMember member, CancellationToken cancellationToken)
        {
            await dbContext.WorkspaceMembers.AddAsync(member, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken) =>
            dbContext.SaveChangesAsync(cancellationToken);

        public Task UpdateMemberAsync(WorkspaceMember member, CancellationToken cancellationToken) =>
            dbContext.SaveChangesAsync(cancellationToken);

        public Task RemoveAsync(Workspace workspace, CancellationToken cancellationToken)
        {
            dbContext.Workspaces.Remove(workspace);
            return dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task RemoveMemberAsync(WorkspaceMember member, CancellationToken cancellationToken)
        {
            dbContext.WorkspaceMembers.Remove(member);
            return dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
