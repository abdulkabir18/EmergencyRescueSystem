using Application.Common.Dtos;
using Application.Features.Incidents.Commands.AcceptIncident;
using Application.Features.Incidents.Commands.CreateIncident;
using Application.Features.Incidents.Dtos;
using Application.Features.Incidents.Queries.GetAllIncidents;
using Application.Features.Incidents.Queries.GetCurrentUserIncidents;
using Application.Features.Incidents.Queries.GetIncidentById;
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
        public async Task<ActionResult<Result<Guid>>> CreateIncident([FromForm] CreateIncidentCommand command, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { version = "1.0", id = result.Data }, result);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get incident by ID",
            Description = "Retrieves the details of a specific incident by its unique identifier."
        )]
        [ProducesResponseType(typeof(Result<IncidentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<IncidentDto>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetIncidentByIdQuery(id), cancellationToken);

            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("all")]
        [SwaggerOperation(
            Summary = "Get all incidents (paginated)"
        )]
        [ProducesResponseType(typeof(Result<PaginatedResult<IncidentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<PaginatedResult<IncidentDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<PaginatedResult<IncidentDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = new GetAllIncidentsQuery(pageNumber, pageSize);
            var result = await mediator.Send(query, cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation(Summary = "Get current user's incidents (paginated)")]
        [ProducesResponseType(typeof(Result<PaginatedResult<IncidentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<PaginatedResult<IncidentDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<PaginatedResult<IncidentDto>>>> GetMine([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = new GetCurrentUserIncidentsQuery(pageNumber, pageSize);
            var result = await mediator.Send(query, cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Responder")]
        [HttpPost("{id:guid}/accept")]
        [SwaggerOperation(
            Summary = "Accept an incident (responder)"
        )]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<Guid>>> AcceptIncident(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new AcceptIncidentCommand(id), cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
