using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Tasks.UpdateTaskStatus;
using TaskFlow.Domain.Entities;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.UnitTests.Handlers.Tasks;

public class UpdateTaskStatusCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _tasks = new();

    private static TaskItem TaskWithStatus(TaskStatus status) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = Guid.NewGuid(),
        Title = "Task",
        Status = status
    };

    [Fact]
    public async Task Handle_SettingDone_ShouldFillCompletedAt()
    {
        var task = TaskWithStatus(TaskStatus.InProgress);
        _tasks.Setup(t => t.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new UpdateTaskStatusCommandHandler(_tasks.Object);

        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, TaskStatus.Done), CancellationToken.None);

        result.Status.Should().Be(TaskStatus.Done);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_MovingAwayFromDone_ShouldClearCompletedAt()
    {
        var task = TaskWithStatus(TaskStatus.Done);
        task.CompletedAt = DateTime.UtcNow;
        _tasks.Setup(t => t.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new UpdateTaskStatusCommandHandler(_tasks.Object);

        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, TaskStatus.Todo), CancellationToken.None);

        result.Status.Should().Be(TaskStatus.Todo);
        result.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReSettingDone_ShouldKeepOriginalCompletedAt()
    {
        var task = TaskWithStatus(TaskStatus.Done);
        var original = DateTime.UtcNow.AddMinutes(-10);
        task.CompletedAt = original;
        _tasks.Setup(t => t.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new UpdateTaskStatusCommandHandler(_tasks.Object);

        var result = await handler.Handle(new UpdateTaskStatusCommand(task.Id, TaskStatus.Done), CancellationToken.None);

        result.CompletedAt.Should().Be(original);
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_ShouldThrowNotFound()
    {
        _tasks.Setup(t => t.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        var handler = new UpdateTaskStatusCommandHandler(_tasks.Object);

        var act = () => handler.Handle(new UpdateTaskStatusCommand(Guid.NewGuid(), TaskStatus.Done), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
