using FluentValidation;
using TaskFlow.Application.Features.Tasks.UpdateTask;
using TaskPriority = TaskFlow.Domain.Entities.TaskPriority;

namespace TaskFlow.Application.Features.Tasks.UpdateTask
{
    public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
    {
        public UpdateTaskCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => x.Description is not null);

            RuleFor(x => x.Priority)
                .IsInEnum();

            RuleFor(x => x.DueDate)
                .Must(dueDate => dueDate is null || dueDate > DateTime.UtcNow)
                .WithMessage("Due date must be in the future.");
        }
    }
}
