using FluentValidation;
using TaskFlow.Application.Features.Workspaces.UpdateWorkspace;

namespace TaskFlow.Application.Features.Workspaces.UpdateWorkspace
{
    public class UpdateWorkspaceCommandValidator : AbstractValidator<UpdateWorkspaceCommand>
    {
        public UpdateWorkspaceCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
