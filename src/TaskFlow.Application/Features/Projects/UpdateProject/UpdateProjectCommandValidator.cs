using FluentValidation;
using TaskFlow.Application.Features.Projects.UpdateProject;

namespace TaskFlow.Application.Features.Projects.UpdateProject
{
    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator()
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
