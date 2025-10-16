namespace Domain.ValueObjects
{
    public class Address
    {
        public string Street { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string LGA { get; private set; }
        public string Country { get; private set; }
        public string PostalCode { get; private set; }

        private Address() { }

        public Address(string street, string city, string state, string lGA, string country, string postalCode)
        {
            Street = street;
            City = city;
            State = state;
            LGA = lGA;
            Country = country;
            PostalCode = postalCode;
        }

        public string ToFullAddress()
        {
            var parts = new List<string?>
            {
                Street,
                City,
                State,
                Country,
                PostalCode
            };

            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        protected IEnumerable<object> GetEqualityComponents()
        {
            yield return Street ?? string.Empty;
            yield return City ?? string.Empty;
            yield return State ?? string.Empty;
            yield return Country ?? string.Empty;
            yield return PostalCode ?? string.Empty;
        }
    }
}
