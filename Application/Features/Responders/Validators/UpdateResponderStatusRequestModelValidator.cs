using Application.Features.Responders.Dtos;
using FluentValidation;

namespace Application.Features.Responders.Validators
{
    public class UpdateResponderStatusRequestModelValidator : AbstractValidator<UpdateResponderStatusRequestModel>
    {
        public UpdateResponderStatusRequestModelValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid responder status.");
        }
    }
}