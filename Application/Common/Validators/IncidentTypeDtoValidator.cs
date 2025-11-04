using Application.Common.Dtos;
using FluentValidation;

namespace Application.Common.Validators
{
    public class IncidentTypeDtoValidator : AbstractValidator<IncidentTypeDto>
    {
        public IncidentTypeDtoValidator()
        {
            RuleFor(x => x.AcceptedIncidentType)
                .IsInEnum().WithMessage("Invalid incident type.");
        }
    }
}
