using Application.Common.Dtos;
using Application.Features.Responders.Dtos;
using MediatR;

namespace Application.Features.Responders.Queries.GetCurrentUserResponder
{
    public record GetCurrentUserResponderQuery() : IRequest<Result<ResponderDto>>;
}