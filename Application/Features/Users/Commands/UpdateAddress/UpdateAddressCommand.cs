using Application.Common.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.UpdateAddress
{
    public record UpdateAddressCommand(AddressDto Address) : IRequest<Result<Unit>>;
}
