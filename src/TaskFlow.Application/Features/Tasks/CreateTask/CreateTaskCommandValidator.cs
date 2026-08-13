using FluentValidation;
using TaskFlow.Application.Features.Tasks.CreateTask;
using TaskStatus = TaskFlow.Domain.Entities.TaskStatus;
using TaskPriority = TaskFlow.Domain.Entities.TaskPriority;

namespace TaskFlow.Application.Features.Tasks.CreateTask
{
    public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
    {
        public CreateTaskCommandValidator()
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
