namespace Application.Features.Responders.Dtos
{
    public record UpdateResponderLocationRequestModel
    {
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }
}