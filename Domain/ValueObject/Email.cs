using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public sealed class Email : IEquatable<Email>
    {
        public string Value { get; private set; }

        private static readonly Regex EmailRegex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private Email() { }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !EmailRegex.IsMatch(value))
                throw new ArgumentException("Invalid email format.", nameof(value));

            Value = value.ToLowerInvariant();
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj) => Equals(obj as Email);

        public bool Equals(Email? other) => other is not null && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public static implicit operator string(Email email) => email.Value;

        public static explicit operator Email(string value) => new(value);
    }
}
