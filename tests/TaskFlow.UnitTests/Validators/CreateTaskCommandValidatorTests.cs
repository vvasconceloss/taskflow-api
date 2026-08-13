using FluentAssertions;
using TaskFlow.Application.Features.Tasks.CreateTask;
using TaskFlow.Domain.Entities;
using TaskPriority = TaskFlow.Domain.Entities.TaskPriority;

namespace TaskFlow.UnitTests.Validators;

public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(new CreateTaskCommand(
            Guid.NewGuid(), "Task", null, TaskPriority.Low, DateTime.UtcNow.AddDays(1)));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPastDueDate_ShouldFail()
    {
        var result = _validator.Validate(new CreateTaskCommand(
            Guid.NewGuid(), "Task", null, TaskPriority.Low, DateTime.UtcNow.AddDays(-1)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldFail()
    {
        var result = _validator.Validate(new CreateTaskCommand(
            Guid.NewGuid(), "", null, TaskPriority.Low, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_WithOnlyName_ShouldPass()
    {
        var result = _validator.Validate(new CreateTaskCommand(Guid.NewGuid(), "Task", null, TaskPriority.Low, null));

        result.IsValid.Should().BeTrue();
    }
}
