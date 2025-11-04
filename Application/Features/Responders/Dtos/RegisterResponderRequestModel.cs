using Application.Common.Dtos;
using Application.Features.Users.Dtos;

namespace Application.Features.Responders.Dtos
{
    public record RegisterResponderRequestModel(RegisterUserRequestModel RegisterUserRequest, Guid AgencyId, GeoLocationDto? AssignedLocation);
}