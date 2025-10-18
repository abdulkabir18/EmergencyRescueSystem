using Application.Features.Users.Dtos;
using FluentValidation;

namespace Application.Features.Auth.Validators
{
    public class GoogleLoginRequestModelValidator : AbstractValidator<GoogleLoginRequestModel>
    {
        public GoogleLoginRequestModelValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token is required.")
                .Must(token => !string.IsNullOrWhiteSpace(token))
                .WithMessage("Access token cannot be whitespace.");
        }
    }

}
