using Application.Features.Users.Dtos;
using FluentValidation;

namespace Application.Features.Users.Validators
{
    public class UpdateFullNameRequestModelValidator : AbstractValidator<UpdateFullNameRequestModel>
    {
        public UpdateFullNameRequestModelValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters long.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
                .Matches("^[A-Za-z ,.'-]+$").WithMessage("First name contains invalid characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters long.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
                .Matches("^[A-Za-z ,.'-]+$").WithMessage("Last name contains invalid characters.");
        }
    }
}
