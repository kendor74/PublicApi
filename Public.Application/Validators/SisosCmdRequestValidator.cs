using FluentValidation;
using Public.Application.DTO;
using System.Text.Json;

namespace Public.Application.Validators
{
    public class SisosCmdRequestValidator : AbstractValidator<SisosCmdRequest>
    {
        private static readonly string[] AllowedCommands =
        {
            "RepoLifePolicy",
            "RepoClaim",
            "RepoContact"
        };

        public SisosCmdRequestValidator()
        {
            RuleFor(x => x.Command)
                .NotEmpty()
                .WithMessage("Command is required.")
                .Must(command => AllowedCommands.Contains(command))
                .WithMessage("Command must be one of: RepoLifePolicy, RepoClaim, RepoContact.");

            RuleFor(x => x.Context.ValueKind)
                .NotEqual(JsonValueKind.Undefined)
                .WithMessage("Context is required.")
                .NotEqual(JsonValueKind.Null)
                .WithMessage("Context cannot be null.")
                .Equal(JsonValueKind.Object)
                .WithMessage("Context must be a JSON object.");
        }
    }
}