using FluentValidation;
using Public.Application.DTO;

namespace Public.Application.Validators
{
    public class SisosLoginRequestValidator : AbstractValidator<SisosLoginRequest>
    {
        public SisosLoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email format is invalid.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}