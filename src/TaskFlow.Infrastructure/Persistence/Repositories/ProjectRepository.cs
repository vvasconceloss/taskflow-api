using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class ProjectRepository(ApplicationDbContext dbContext) : IProjectRepository
    {
        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        public Task<List<Project>> GetProjectsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken) =>
            dbContext.Projects
                .Where(p => p.WorkspaceId == workspaceId && !p.IsArchived)
                .OrderBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public Task<Guid?> GetWorkspaceIdByProjectIdAsync(Guid projectId, CancellationToken cancellationToken) =>
            dbContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => (Guid?)p.WorkspaceId)
                .FirstOrDefaultAsync(cancellationToken);

        public Task<bool> HasTasksAsync(Guid projectId, CancellationToken cancellationToken) =>
            dbContext.TaskItems.AnyAsync(t => t.ProjectId == projectId, cancellationToken);

        public async Task CreateAsync(Project project, CancellationToken cancellationToken)
        {
            await dbContext.Projects.AddAsync(project, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task UpdateAsync(Project project, CancellationToken cancellationToken) =>
            dbContext.SaveChangesAsync(cancellationToken);

        public Task RemoveAsync(Project project, CancellationToken cancellationToken)
        {
            dbContext.Projects.Remove(project);
            return dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
