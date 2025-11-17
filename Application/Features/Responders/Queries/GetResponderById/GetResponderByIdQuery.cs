using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using MediatR;

namespace Application.Features.Responders.Queries.GetResponderById
{
    public record GetResponderByIdQuery(Guid ResponderId) : IRequest<Result<ResponderDto>>;
}