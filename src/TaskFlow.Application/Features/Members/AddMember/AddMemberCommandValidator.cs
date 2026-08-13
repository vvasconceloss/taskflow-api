using FluentValidation;
using TaskFlow.Application.Features.Members.AddMember;

namespace TaskFlow.Application.Features.Members.AddMember
{
    public class AddMemberCommandValidator : AbstractValidator<AddMemberCommand>
    {
        public AddMemberCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
