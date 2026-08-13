using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Features.Tasks.ListTasks;
using TaskFlow.Domain.Entities;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.UnitTests.Handlers.Tasks;

public class ListTasksQueryHandlerTests
{
    private readonly Mock<ITaskRepository> _tasks = new();

    private static readonly PagedResult<TaskItem> Empty =
        new(new List<TaskItem>(), 1, 20, 0, 0);

    private ListTasksQueryHandler CreateHandler() => new(_tasks.Object);

    [Fact]
    public async Task Handle_WithInvalidSortField_ShouldThrowValidation()
    {
        _tasks.Setup(t => t.GetTasksAsync(It.IsAny<Guid>(), It.IsAny<TaskStatus?>(), It.IsAny<TaskPriority?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Empty);
        var handler = CreateHandler();
        var query = new ListTasksQuery(Guid.NewGuid(), null, null, null, 1, 20, "hack;drop", false);

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithPageSizeAboveMax_ShouldClampToMax()
    {
        var handler = CreateHandler();
        _tasks.Setup(t => t.GetTasksAsync(
                It.IsAny<Guid>(),
                It.IsAny<TaskStatus?>(),
                It.IsAny<TaskPriority?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Empty);

        await handler.Handle(new ListTasksQuery(Guid.NewGuid(), null, null, null, 1, 1000, null, false), CancellationToken.None);

        _tasks.Verify(t => t.GetTasksAsync(
            It.IsAny<Guid>(), It.IsAny<TaskStatus?>(), It.IsAny<TaskPriority?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<int>(), 100, It.IsAny<CancellationToken>()), Times.Once);
    }
}
