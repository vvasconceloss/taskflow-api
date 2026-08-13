using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.ListProjects
{
    public record ListProjectsQuery(Guid WorkspaceId, int Page, int PageSize, string? SortBy, bool SortDescending)
        : IWorkspaceScoped, IRequest<PagedResult<Project>>;
}
