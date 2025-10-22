using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Users.Queries.GetAllUser
{
    public record GetAllUserQuery(int PageNumber, int PageSize) : IRequest<PaginatedResult<UserDto>>;
}
