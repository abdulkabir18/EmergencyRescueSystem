using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Users.Queries.GetProfile
{
    public record GetProfileQuery() : IRequest<Result<UserProfileDto>>;
}
