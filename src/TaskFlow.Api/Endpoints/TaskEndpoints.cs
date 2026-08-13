using MediatR;
using TaskFlow.Api.Contracts;
using TaskFlow.Application.Features.Tasks.CreateTask;
using TaskFlow.Application.Features.Tasks.DeleteTask;
using TaskFlow.Application.Features.Tasks.GetTask;
using TaskFlow.Application.Features.Tasks.ListTasks;
using TaskFlow.Application.Features.Tasks.UpdateTask;
using TaskFlow.Application.Features.Tasks.UpdateTaskAssignee;
using TaskFlow.Application.Features.Tasks.UpdateTaskStatus;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Api.Endpoints
{
    public static class TaskEndpoints
    {
        public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
        {
            var projectGroup = app.MapGroup("/projects/{projectId:guid}/tasks").RequireAuthorization();

            projectGroup.MapGet("/", async (Guid projectId, ISender sender) =>
            {
                var tasks = await sender.Send(new ListTasksQuery(projectId));
                return Results.Ok(tasks.Select(ToResponse));
            });

            projectGroup.MapPost("/", async (Guid projectId, CreateTaskRequest request, ISender sender) =>
            {
                var task = await sender.Send(new CreateTaskCommand(
                    projectId, request.Title, request.Description, request.Priority ?? TaskPriority.Low, request.DueDate));
                return Results.Created(string.Empty, ToResponse(task));
            });

            var taskGroup = app.MapGroup("/tasks").RequireAuthorization();

            taskGroup.MapGet("/{taskId:guid}", async (Guid taskId, ISender sender) =>
            {
                var task = await sender.Send(new GetTaskQuery(taskId));
                return Results.Ok(ToResponse(task));
            });

            taskGroup.MapPatch("/{taskId:guid}", async (Guid taskId, UpdateTaskRequest request, ISender sender) =>
            {
                var task = await sender.Send(new UpdateTaskCommand(taskId, request.Title, request.Description, request.Priority, request.DueDate));
                return Results.Ok(ToResponse(task));
            });

            taskGroup.MapPatch("/{taskId:guid}/status", async (Guid taskId, UpdateTaskStatusRequest request, ISender sender) =>
            {
                var task = await sender.Send(new UpdateTaskStatusCommand(taskId, request.Status));
                return Results.Ok(ToResponse(task));
            });

            taskGroup.MapPatch("/{taskId:guid}/assignee", async (Guid taskId, UpdateTaskAssigneeRequest request, ISender sender) =>
            {
                var task = await sender.Send(new UpdateTaskAssigneeCommand(taskId, request.AssigneeUserId));
                return Results.Ok(ToResponse(task));
            });

            taskGroup.MapDelete("/{taskId:guid}", async (Guid taskId, ISender sender) =>
            {
                await sender.Send(new DeleteTaskCommand(taskId));
                return Results.NoContent();
            });

            return app;
        }

        private static TaskResponse ToResponse(TaskItem task) =>
            new(
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.Priority,
                task.ProjectId,
                task.AssigneeUserId,
                task.DueDate,
                task.CreatedAt,
                task.CompletedAt);
    }
}
