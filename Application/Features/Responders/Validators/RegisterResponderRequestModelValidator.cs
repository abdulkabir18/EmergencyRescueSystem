using Application.Features.Responders.Dtos;
using Application.Features.Users.Validators;
using FluentValidation;

namespace Application.Features.Responders.Validators
{
    public class RegisterResponderRequestModelValidator : AbstractValidator<RegisterResponderRequestModel>
    {
        public RegisterResponderRequestModelValidator()
        {
            RuleFor(x => x.RegisterUserRequest)
                .NotNull().WithMessage("Responder user information is required.")
                .SetValidator(new RegisterUserRequestModelValidator());

            RuleFor(x => x.AgencyId)
                .NotEmpty().WithMessage("AgencyId is required.")
                .Must(id => id != Guid.Empty)
                .WithMessage("AgencyId cannot be an empty GUID.");

            When(x => x.AssignedLocation != null, () =>
            {
                RuleFor(x => x.AssignedLocation!.Latitude)
                    .InclusiveBetween(-90, 90)
                    .WithMessage("Latitude must be between -90 and 90 degrees.");

                RuleFor(x => x.AssignedLocation!.Longitude)
                    .InclusiveBetween(-180, 180)
                    .WithMessage("Longitude must be between -180 and 180 degrees.");
            });
        }
    }
}