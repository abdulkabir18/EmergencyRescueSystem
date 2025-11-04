using Application.Common.Dtos;
using MediatR;

namespace Application.Features.Users.Queries.GetTotalUserCount
{
    public record GetTotalUserCountQuery : IRequest<Result<int>>;
}
