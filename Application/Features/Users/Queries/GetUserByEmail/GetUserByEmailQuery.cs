using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using MediatR;

namespace Application.Features.Users.Queries.GetUserByEmail
{
    public record GetUserByEmailQuery(string Email) : IRequest<Result<UserDto>>;
}
