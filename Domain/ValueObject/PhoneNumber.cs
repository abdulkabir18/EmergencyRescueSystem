using System.Text.RegularExpressions;

namespace Domain.ValueObject
{
    public sealed class PhoneNumber : IEquatable<PhoneNumber>
    {
        public string Value { get; } = default!;

        private static readonly Regex PhoneNumberRegex =
            new(@"^(?:\+234|0)(7[0-9]|8[0-9]|9[0-9])\d{8}$",
                RegexOptions.Compiled);

        private PhoneNumber() { }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number is required.", nameof(value));

            value = value.Trim();

            if (!PhoneNumberRegex.IsMatch(value))
                throw new ArgumentException("Invalid Nigerian phone number format.", nameof(value));

            Value = Normalize(value);
        }

        private static string Normalize(string value)
        {
            return value.StartsWith("0")
                ? "+234" + value[1..]
                : value;
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj) => Equals(obj as PhoneNumber);

        public bool Equals(PhoneNumber? other) =>
            other is not null && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public static implicit operator string(PhoneNumber phone) => phone.Value;

        public static explicit operator PhoneNumber(string value) => new(value);

        public static bool operator ==(PhoneNumber? left, PhoneNumber? right) => Equals(left, right);

        public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !Equals(left, right);
    }
}
