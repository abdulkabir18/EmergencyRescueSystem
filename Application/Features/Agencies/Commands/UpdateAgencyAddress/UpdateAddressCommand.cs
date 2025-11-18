using Application.Common.Dtos;
using MediatR;

namespace Application.Features.Agencies.Commands.UpdateAgencyAddress
{
    public record UpdateAddressCommand(AddressDto Address) : IRequest<Result<Unit>>;
}
