using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Features.Users.Queries.GetAllUserByRole
{
    public record GetAllUserByRoleQuery(UserRole Role,int pageNumber,int pageSize) : IRequest<PaginatedResult<UserDto>>;
}
