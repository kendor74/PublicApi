using FluentValidation;
using Public.Application.DTO;

namespace Public.Application.Validators
{
    public class SisosLoginRequestValidator : AbstractValidator<SisosLoginRequest>
    {
        public SisosLoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email is required.")
                .Must(email => email.StartsWith(@"mic\", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Email must start with 'mic\\'.")
                .Must(email =>
                {
                    var emailPart = email.Substring(@"mic\".Length);

                    return !string.IsNullOrWhiteSpace(emailPart)
                           && new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                               .IsValid(emailPart);
                })
                .WithMessage("Email format is invalid. Expected format: mic\\email@example.com.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}