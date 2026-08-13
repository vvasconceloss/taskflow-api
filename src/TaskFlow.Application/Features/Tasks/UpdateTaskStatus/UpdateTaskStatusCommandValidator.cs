using FluentValidation;
using TaskFlow.Application.Features.Tasks.UpdateTaskStatus;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.UpdateTaskStatus
{
    public class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
    {
        public UpdateTaskStatusCommandValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum();
        }
    }
}
