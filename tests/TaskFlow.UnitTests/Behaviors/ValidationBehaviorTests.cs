using FluentAssertions;
using FluentValidation;
using MediatR;
using TaskFlow.Application.Common.Behaviors;
using ValidationException = TaskFlow.Application.Common.Exceptions.ValidationException;

namespace TaskFlow.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    private sealed record SampleRequest(string Name) : IRequest<Unit>;

    private sealed class SampleValidator : AbstractValidator<SampleRequest>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        var behavior = new ValidationBehavior<SampleRequest, Unit>(new[] { new SampleValidator() });

        var act = () => behavior.Handle(
            new SampleRequest(""),
            _ => Task.FromResult(Unit.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldContinue()
    {
        var behavior = new ValidationBehavior<SampleRequest, Unit>(new[] { new SampleValidator() });
        var nextCalled = false;

        await behavior.Handle(
            new SampleRequest("valid"),
            _ => { nextCalled = true; return Task.FromResult(Unit.Value); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithoutValidators_ShouldContinue()
    {
        var behavior = new ValidationBehavior<SampleRequest, Unit>(Array.Empty<IValidator<SampleRequest>>());
        var nextCalled = false;

        await behavior.Handle(
            new SampleRequest("anything"),
            _ => { nextCalled = true; return Task.FromResult(Unit.Value); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
    }
}
