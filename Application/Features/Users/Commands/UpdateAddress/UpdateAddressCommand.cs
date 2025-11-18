using Application.Common.Dtos;
using MediatR;

namespace Application.Features.Users.Commands.UpdateAddress
{
    public record UpdateAddressCommand(AddressDto Address) : IRequest<Result<Unit>>;
}
