using Application.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Agencies.Commands.SetLogo
{
    public record SetLogoCommand(IFormFile Logo) : IRequest<Result<Unit>>;
}
