using Application.Common.Validators;
using Application.Features.Agencies.Dtos;
using FluentValidation;

namespace Application.Features.Agencies.Validators
{
    public class AddSupportedIncidentRequestModelValidator : AbstractValidator<AddSupportedIncidentRequestModel>
    {
        public AddSupportedIncidentRequestModelValidator()
        {
            RuleFor(x => x.AgencyId)
                .NotEmpty().WithMessage("Agency ID is required.");

            RuleFor(x => x.TypeDto)
                .NotNull().WithMessage("Incident type is required.")
                .SetValidator(new IncidentTypeDtoValidator());
        }
    }
}