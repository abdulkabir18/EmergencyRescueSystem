namespace Application.Features.Incidents.Dtos
{
    public record AssignResponderRequestModel(Guid ResponderId, Guid IncidentId);
}
