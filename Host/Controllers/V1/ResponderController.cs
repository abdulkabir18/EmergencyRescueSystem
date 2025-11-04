using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ResponderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ResponderController(IMediator mediator)
        {
            _mediator = mediator;
        }


    }
}
