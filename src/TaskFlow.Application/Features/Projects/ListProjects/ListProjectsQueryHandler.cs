using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.ListProjects
{
    public class ListProjectsQueryHandler(IProjectRepository projects)
        : IRequestHandler<ListProjectsQuery, PagedResult<Project>>
    {
        private static readonly HashSet<string> AllowedSortFields = ["name", "createdAt"];
        private const int MaxPageSize = 100;

        public Task<PagedResult<Project>> Handle(ListProjectsQuery request, CancellationToken cancellationToken)
        {
            if (request.SortBy is not null && !AllowedSortFields.Contains(request.SortBy))
            {
                throw new ValidationException($"Sort field '{request.SortBy}' is not allowed.");
            }

            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

            return projects.GetProjectsAsync(
                request.WorkspaceId,
                request.SortBy,
                request.SortDescending,
                page,
                pageSize,
                cancellationToken);
        }
    }
}
