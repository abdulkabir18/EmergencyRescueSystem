namespace Application.Common.Dtos
{
    public record AddressDto
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? LGA { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
    }
}