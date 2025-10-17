using Application.Common.Constants;
using Application.Common.Dtos;
using FluentValidation;

namespace Application.Common.Validators
{
    public class AddressDtoValidator : AbstractValidator<AddressDto>
    {
        public AddressDtoValidator()
        {
            When(HasAnyAddressField, () =>
            {
                RuleFor(x => x.Street)
                    .NotEmpty().WithMessage("Street is required.")
                    .Length(3, 100).WithMessage("Street must be between 3 and 100 characters.");

                RuleFor(x => x.State)
                    .NotEmpty().WithMessage("State is required.")
                    .Must(state => NigeriaData.States.Contains(state!, StringComparer.OrdinalIgnoreCase))
                    .WithMessage("Invalid state. Must be one of Nigeria's 36 states or FCT.");

                RuleFor(x => x.City)
                    .NotEmpty().WithMessage("City is required.")
                    .Must((address, city) =>
                        !string.IsNullOrWhiteSpace(address.State) &&
                        NigeriaData.Cities.TryGetValue(address.State, out var cities) &&
                        cities.Any(c => c.Equals(city, StringComparison.OrdinalIgnoreCase)))
                    .WithMessage((address, city) => $"City '{city}' is not valid for state '{address.State}'.")
                    .When(x => !string.IsNullOrWhiteSpace(x.State));

                RuleFor(x => x.LGA)
                    .NotEmpty().WithMessage("LGA is required.")
                    .Must((address, lga) =>
                        !string.IsNullOrWhiteSpace(address.State) &&
                        NigeriaData.LGAs.TryGetValue(address.State, out var lgas) &&
                        lgas.Any(l => l.Equals(lga, StringComparison.OrdinalIgnoreCase)))
                    .WithMessage((address, lga) => $"LGA '{lga}' is not valid for state '{address.State}'.")
                    .When(x => !string.IsNullOrWhiteSpace(x.State));

                RuleFor(x => x.PostalCode)
                    .NotEmpty().WithMessage("Postal code is required.")
                    .Matches(@"^\d{6}$").WithMessage("Postal code must be 6 digits.");

                RuleFor(x => x.Country)
                    .NotEmpty().WithMessage("Country is required.")
                    .Must(country => string.Equals(country?.Trim(), "Nigeria", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(country?.Trim(), "NG", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("Country must be Nigeria (or NG).");
            });
        }

        private static bool HasAnyAddressField(AddressDto dto)
        {
            return !string.IsNullOrWhiteSpace(dto.Street)
                || !string.IsNullOrWhiteSpace(dto.City)
                || !string.IsNullOrWhiteSpace(dto.State)
                || !string.IsNullOrWhiteSpace(dto.LGA)
                || !string.IsNullOrWhiteSpace(dto.Country)
                || !string.IsNullOrWhiteSpace(dto.PostalCode);
        }
    }
}
