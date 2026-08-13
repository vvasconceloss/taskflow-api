using FluentValidation;
using TaskFlow.Application.Features.Auth.Register;

namespace TaskFlow.Application.Features.Auth.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .Length(8, 72)
                .Must(password => password.Any(char.IsLetter) && password.Any(char.IsDigit))
                .WithMessage("Password must contain at least one letter and one number.");

            RuleFor(x => x.Name)
                .MaximumLength(100)
                .When(x => x.Name is not null);
        }
    }
}
