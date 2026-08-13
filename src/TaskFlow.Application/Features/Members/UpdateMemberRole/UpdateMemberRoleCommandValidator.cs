using FluentValidation;
using TaskFlow.Application.Features.Members.UpdateMemberRole;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Members.UpdateMemberRole
{
    public class UpdateMemberRoleCommandValidator : AbstractValidator<UpdateMemberRoleCommand>
    {
        public UpdateMemberRoleCommandValidator()
        {
            RuleFor(x => x.Role)
                .IsInEnum();
        }
    }
}
