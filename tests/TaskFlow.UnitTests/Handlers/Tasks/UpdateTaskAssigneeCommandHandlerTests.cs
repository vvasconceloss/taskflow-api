using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Tasks.UpdateTaskAssignee;
using TaskFlow.Domain.Entities;

namespace TaskFlow.UnitTests.Handlers.Tasks;

public class UpdateTaskAssigneeCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IWorkspaceRepository> _workspaces = new();

    private static TaskItem Task() => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = Guid.NewGuid(),
        Title = "Task"
    };

    [Fact]
    public async Task Handle_WhenAssigneeIsMember_ShouldSetAssignee()
    {
        var task = Task();
        _tasks.Setup(t => t.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _tasks.Setup(t => t.GetWorkspaceIdByTaskIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        _workspaces.Setup(w => w.IsMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var assigneeId = Guid.NewGuid();
        var handler = new UpdateTaskAssigneeCommandHandler(_tasks.Object, _workspaces.Object);

        var result = await handler.Handle(new UpdateTaskAssigneeCommand(task.Id, assigneeId), CancellationToken.None);

        result.AssigneeUserId.Should().Be(assigneeId);
    }

    [Fact]
    public async Task Handle_WhenAssigneeIsNotMember_ShouldThrowForbidden()
    {
        var task = Task();
        _tasks.Setup(t => t.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _tasks.Setup(t => t.GetWorkspaceIdByTaskIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        _workspaces.Setup(w => w.IsMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new UpdateTaskAssigneeCommandHandler(_tasks.Object, _workspaces.Object);

        var act = () => handler.Handle(new UpdateTaskAssigneeCommand(task.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _tasks.Verify(t => t.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RemovingAssignee_ShouldClear()
    {
        var task = Task();
        task.AssigneeUserId = Guid.NewGuid();
        _tasks.Setup(t => t.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var handler = new UpdateTaskAssigneeCommandHandler(_tasks.Object, _workspaces.Object);

        var result = await handler.Handle(new UpdateTaskAssigneeCommand(task.Id, null), CancellationToken.None);

        result.AssigneeUserId.Should().BeNull();
        _workspaces.Verify(w => w.IsMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
