using Domain.Enums;

namespace Application.Features.Responders.Dtos
{
    public record UpdateResponderStatusRequestModel
    {
        public ResponderStatus Status { get; init; }
    }
}