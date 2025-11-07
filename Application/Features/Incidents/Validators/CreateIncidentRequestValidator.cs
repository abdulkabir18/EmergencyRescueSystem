using Application.Common.Validators;
using Application.Features.Incidents.Dtos;
using FluentValidation;

namespace Application.Features.Incidents.Validators
{
    public class CreateIncidentRequestValidator : AbstractValidator<CreateIncidentRequestModel>
    {
        private const int MaxFileSizeInMB = 25;
        private static readonly string[] AllowedExtensions =
            [".jpg", ".jpeg", ".png", ".mp4", ".mov", ".avi", ".mp3", ".wav", ".m4a"];

        public CreateIncidentRequestValidator()
        {
            RuleFor(x => x.Coordinate)
                .NotNull().WithMessage("Coordinate is required.")
                .SetValidator(new GeoLocationDtoValidator());

            RuleFor(x => x.Prove)
                .NotNull().WithMessage("A media file is required as proof.")
                .Must(f => f.Length > 0).WithMessage("File cannot be empty.")
                .Must(f => f.Length <= MaxFileSizeInMB * 1024 * 1024)
                .WithMessage($"File size must not exceed {MaxFileSizeInMB} MB.")
                .Must(f => IsSupportedFileType(f.FileName))
                .WithMessage("Unsupported file type. Allowed types: image, video, or audio formats only.");

            RuleFor(x => x.OccurredAt)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-15))
                .WithMessage("OccurredAt must be within the last 15 minutes and cannot be a future time.");
        }

        private static bool IsSupportedFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }
    }
}
