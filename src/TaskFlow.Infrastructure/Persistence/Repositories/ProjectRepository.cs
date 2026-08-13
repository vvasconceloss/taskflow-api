using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class ProjectRepository(ApplicationDbContext dbContext) : IProjectRepository
    {
        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        public async Task<PagedResult<Project>> GetProjectsAsync(
            Guid workspaceId,
            string? sortBy,
            bool sortDescending,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = dbContext.Projects.Where(p => p.WorkspaceId == workspaceId && !p.IsArchived);

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await ApplySort(query, sortBy, sortDescending)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PagedResult<Project>(
                items,
                page,
                pageSize,
                totalItems,
                (int)Math.Ceiling(totalItems / (double)pageSize));
        }

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

        private static IQueryable<Project> ApplySort(IQueryable<Project> query, string? sortBy, bool descending) =>
            sortBy switch
            {
                "name" => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                _ => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };
    }
}
