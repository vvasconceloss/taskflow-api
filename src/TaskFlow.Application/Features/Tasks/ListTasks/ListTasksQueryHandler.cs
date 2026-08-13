using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.ListTasks
{
    public class ListTasksQueryHandler(ITaskRepository tasks)
        : IRequestHandler<ListTasksQuery, PagedResult<TaskItem>>
    {
        private static readonly HashSet<string> AllowedSortFields = ["title", "status", "priority", "dueDate", "createdAt"];
        private const int MaxPageSize = 100;

        public Task<PagedResult<TaskItem>> Handle(ListTasksQuery request, CancellationToken cancellationToken)
        {
            if (request.SortBy is not null && !AllowedSortFields.Contains(request.SortBy))
            {
                throw new ValidationException($"Sort field '{request.SortBy}' is not allowed.");
            }

            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

            return tasks.GetTasksAsync(
                request.ProjectId,
                request.Status,
                request.Priority,
                request.AssigneeId,
                request.SortBy,
                request.SortDescending,
                page,
                pageSize,
                cancellationToken);
        }
    }
}
