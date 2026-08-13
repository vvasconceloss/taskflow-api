using FluentValidation;
using TaskFlow.Application.Features.Workspaces.CreateWorkspace;

namespace TaskFlow.Application.Features.Workspaces.CreateWorkspace
{
    public class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
    {
        public CreateWorkspaceCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
