using FluentValidation;
using Public.Application.DTO;

namespace Public.Application.Validators
{
    public class SmsOTPRequestValidator
        : AbstractValidator<SmsOTPRequest>
    {
        public SmsOTPRequestValidator()
        {
            RuleFor(x => x.Mobile)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Mobile number is required.")
                .Matches(@"^20(10|11|12|15)\d{8}$")
                .WithMessage(
                    "Mobile number must be a valid Egyptian mobile number in international format, e.g. 201012345678.");

            RuleFor(x => x.OTP)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("OTP is required.")
                .Matches(@"^\d+$")
                .WithMessage("OTP must contain numbers only.");
        }
    }
}