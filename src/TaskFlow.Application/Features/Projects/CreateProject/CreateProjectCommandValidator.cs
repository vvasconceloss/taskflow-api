using FluentValidation;
using TaskFlow.Application.Features.Projects.CreateProject;

namespace TaskFlow.Application.Features.Projects.CreateProject
{
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => x.Description is not null);
        }
    }
}
