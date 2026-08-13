using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.ListTasks
{
    public class ListTasksQueryHandler(ITaskRepository tasks)
        : IRequestHandler<ListTasksQuery, List<TaskItem>>
    {
        public Task<List<TaskItem>> Handle(ListTasksQuery request, CancellationToken cancellationToken) =>
            tasks.GetTasksByProjectAsync(request.ProjectId, cancellationToken);
    }
}
