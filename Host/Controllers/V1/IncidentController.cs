using Application.Common.Dtos;
using Application.Features.Incidents.Commands.CreateIncident;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Host.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class IncidentController(IMediator mediator) : ControllerBase
    {
        [Authorize]
        [HttpPost("panic-alert")]
        [SwaggerOperation(
            Summary = "Create a new incident"
        )]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Result<Guid>>> CreateIncident([FromForm] CreateIncidentCommand command)
        {
            var result = await mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get incident by ID",
            Description = "Retrieves the details of a specific incident by its unique identifier."
        )]
        [ProducesResponseType(typeof(Result<IncidentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<IncidentDto>>> GetById(Guid id)
        {
            var result = await mediator.Send(new GetIncidentByIdQuery(id));

            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }
    }
}
