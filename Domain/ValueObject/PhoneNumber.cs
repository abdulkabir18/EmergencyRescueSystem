using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public sealed class PhoneNumber : IEquatable<PhoneNumber>
    {
        public string Value { get; private set; }

        private static readonly Regex PhoneNumberRegex =
            new(@"^(?:\+234|0)[789][01]\d{8}$", RegexOptions.Compiled);

        private PhoneNumber() { }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number is required.", nameof(value));

            if (!PhoneNumberRegex.IsMatch(value))
                throw new ArgumentException("Invalid Nigerian phone number format.", nameof(value));

            Value = Normalize(value);
        }

        private static string Normalize(string value)
        {
            if (value.StartsWith("0"))
                return "+234" + value[1..];
            return value;
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj) => Equals(obj as PhoneNumber);

        public bool Equals(PhoneNumber? other) => other is not null && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public static implicit operator string(PhoneNumber phone) => phone.Value;

        public static explicit operator PhoneNumber(string value) => new(value);
    }
}
