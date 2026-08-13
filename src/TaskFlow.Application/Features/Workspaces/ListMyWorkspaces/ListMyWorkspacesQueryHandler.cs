using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.ListMyWorkspaces
{
    public class ListMyWorkspacesQueryHandler(IWorkspaceRepository workspaces, ICurrentUserService currentUser)
        : IRequestHandler<ListMyWorkspacesQuery, PagedResult<Workspace>>
    {
        private static readonly HashSet<string> AllowedSortFields = ["name", "createdAt"];
        private const int MaxPageSize = 100;

        public Task<PagedResult<Workspace>> Handle(ListMyWorkspacesQuery request, CancellationToken cancellationToken)
        {
            if (request.SortBy is not null && !AllowedSortFields.Contains(request.SortBy))
            {
                throw new ValidationException($"Sort field '{request.SortBy}' is not allowed.");
            }

            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

            return workspaces.GetWorkspacesForUserAsync(
                currentUser.UserId,
                request.SortBy,
                request.SortDescending,
                page,
                pageSize,
                cancellationToken);
        }
    }
}
