using Application.Features.Responders.Dtos;
using FluentValidation;

namespace Application.Features.Responders.Validators
{
    public class UpdateResponderLocationRequestModelValidator : AbstractValidator<UpdateResponderLocationRequestModel>
    {
        public UpdateResponderLocationRequestModelValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90 degrees.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180 degrees.");
        }
    }
}