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
                .WithMessage("Chain is required.");

            RuleFor(x => x.Context.ValueKind)
                .NotEqual(JsonValueKind.Undefined)
                .WithMessage("Context is required.")
                .NotEqual(JsonValueKind.Null)
                .WithMessage("Context cannot be null.");
        }
    }
}