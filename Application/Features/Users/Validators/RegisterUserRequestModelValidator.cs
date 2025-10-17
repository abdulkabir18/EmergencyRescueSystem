using Application.Features.Users.Dtos;
using FluentValidation;

namespace Application.Features.Users.Validators
{
    public class RegisterUserRequestModelValidator : AbstractValidator<RegisterUserRequestModel>
    {
        public RegisterUserRequestModelValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .Matches(@"^[a-zA-Z]+$").WithMessage("First name should contain only letters.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .Matches(@"^[a-zA-Z]+$").WithMessage("Last name should contain only letters.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[\W]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Passwords do not match.");

            When(x => x.ProfilePicture != null, () =>
            {
                RuleFor(x => x.ProfilePicture!.Length)
                    .LessThanOrEqualTo(5 * 1024 * 1024) 
                    .WithMessage("Profile picture size cannot exceed 5MB.");

                RuleFor(x => x.ProfilePicture!.ContentType.ToLower())
                    .Must(type => type == "image/jpeg" || type == "image/jpg" || type == "image/png")
                    .WithMessage("Only JPEG, JPG, and PNG images are allowed.");
            });
        }
    }
}
