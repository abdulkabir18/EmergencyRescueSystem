using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Features.Users.Queries.GetAllUserByRole
{
    public record GetAllUserByRoleQuery(UserRole Role,int PageNumber,int PageSize) : IRequest<PaginatedResult<UserDto>>;
}
