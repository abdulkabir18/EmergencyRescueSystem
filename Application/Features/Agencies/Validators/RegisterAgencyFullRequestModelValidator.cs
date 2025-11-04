using Application.Common.Validators;
using Application.Features.Agencies.Dtos;
using Application.Features.Users.Validators;
using FluentValidation;

namespace Application.Features.Agencies.Validators
{
    public class RegisterAgencyFullRequestModelValidator : AbstractValidator<RegisterAgencyFullRequestModel>
    {
        public RegisterAgencyFullRequestModelValidator()
        {
            RuleFor(x => x.RegisterUserRequest)
                .NotNull().WithMessage("User registration details are required.")
                .SetValidator(new RegisterUserRequestModelValidator()); 

            RuleFor(x => x.AgencyName)
                .NotEmpty().WithMessage("Agency name is required.")
                .MaximumLength(100).WithMessage("Agency name cannot exceed 100 characters.");

            RuleFor(x => x.AgencyEmail)
                .NotEmpty().WithMessage("Agency email is required.")
                .EmailAddress().WithMessage("A valid agency email address is required.");

            RuleFor(x => x.AgencyPhoneNumber)
                .NotEmpty().WithMessage("Agency phone number is required.")
                .Matches(@"^(?:\+234|0)[789][01]\d{8}$")
                .WithMessage("Agency phone number must be a valid Nigerian number.");

            When(x => x.AgencyLogo != null, () =>
            {
                RuleFor(x => x.AgencyLogo!.Length)
                    .LessThanOrEqualTo(2 * 1024 * 1024)
                    .WithMessage("Agency logo size cannot exceed 2 MB.");

                RuleFor(x => x.AgencyLogo!.ContentType)
                    .Must(type => type == "image/png" || type == "image/jpeg")
                    .WithMessage("Only PNG or JPEG formats are allowed.");
            });

            When(x => x.AgencyAddress != null, () =>
            {
                RuleFor(x => x.AgencyAddress)
                    .SetValidator(new AddressDtoValidator()!);
            });

            RuleFor(x => x.SupportedIncidents)
                .NotNull().WithMessage("Supported incidents are required.")
                .Must(list => list.Count > 0)
                .WithMessage("At least one supported incident type must be provided.");

            RuleForEach(x => x.SupportedIncidents)
                .Must(value => Enum.TryParse<Domain.Enums.IncidentType>(value, true, out _))
                .WithMessage("{PropertyValue} is not a valid incident type.");
        }
    }
}
