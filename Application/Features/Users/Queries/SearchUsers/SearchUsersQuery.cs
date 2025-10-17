using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Users.Queries.SearchUsers
{
    public record SearchUsersQuery(string Keyword, int PageNumber, int PageSize) : IRequest<PaginatedResult<UserDto>>;

}
