using FluentValidation;
using Public.Application.DTO;
using System.Text.Json;

namespace Public.Application.Validators
{
    public class SisosCustomChainRequestValidator : AbstractValidator<SisosCustomChainRequest>
    {
        public SisosCustomChainRequestValidator()
        {
            RuleFor(x => x.Chain)
                .NotEmpty()
                .WithMessage("Chain is required.")
                .Must(chain => chain.StartsWith("cmd", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Chain must start with 'cmd'.");


            RuleFor(x => x.Payload.ValueKind)
                .NotEqual(JsonValueKind.Undefined)
                .WithMessage("Payload is required.")
                .NotEqual(JsonValueKind.Null)
                .WithMessage("Payload cannot be null.");
        }
    }
}