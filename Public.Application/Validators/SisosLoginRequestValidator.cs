using FluentValidation;
using Public.Application.DTO;
using System.Text.RegularExpressions;

public class SisosLoginRequestValidator
    : AbstractValidator<SisosLoginRequest>
{
    private static readonly Regex MicLifeEmailPattern = new(
        @"^(mic|micuat)\\[a-zA-Z0-9._%+\-]+@(miclife\.com|axxis-systems\.com)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    );

    public SisosLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required.")
            .Must(email =>
                !string.IsNullOrWhiteSpace(email) &&
                MicLifeEmailPattern.IsMatch(email.Trim()))
            .WithMessage(
                @"Email must start with mic\ or micuat\ and end with @miclife.com or @axxis-systems.com."
            );

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}