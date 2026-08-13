using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.ListMyWorkspaces
{
    public record ListMyWorkspacesQuery(int Page, int PageSize, string? SortBy, bool SortDescending)
        : IRequest<PagedResult<Workspace>>;
}
