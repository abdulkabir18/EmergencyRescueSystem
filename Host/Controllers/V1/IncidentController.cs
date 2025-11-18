using Application.Common.Dtos;
using Application.Features.Incidents.Commands.AcceptIncident;
using Application.Features.Incidents.Commands.AssignResponder;
using Application.Features.Incidents.Commands.CancelIncident;
using Application.Features.Incidents.Commands.CreateIncident;
using Application.Features.Incidents.Commands.EscalateIncident;
using Application.Features.Incidents.Commands.MarkInProgress;
using Application.Features.Incidents.Commands.ResolveIncident;
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
        [ProducesResponseType(typeof(PaginatedResult<IncidentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<IncidentDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = new GetAllIncidentsQuery(pageNumber, pageSize);
            var result = await mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation(Summary = "Get current user's incidents (paginated)")]
        [ProducesResponseType(typeof(PaginatedResult<IncidentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<IncidentDto>>> GetMine([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = new GetCurrentUserIncidentsQuery(pageNumber, pageSize);
            var result = await mediator.Send(query, cancellationToken);

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

        [Authorize(Roles = "SuperAdmin, AgencyAdmin")]
        [HttpPost("assign-responder")]
        [SwaggerOperation(
            Summary = "Manually assign a responder to an incident",
            Description = "Allows a SuperAdmin or AgencyAdmin to assign an available responder to an incident."
        )]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Result<Guid>>> AssignResponder([FromBody] AssignResponderRequestModel model)
        {
            var command = new AssignResponderCommand(model);
            var result = await mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize(Roles = "Responder")]
        [HttpPost("{id:guid}/in-progress")]
        [SwaggerOperation(Summary = "Mark incident as IN PROGRESS")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<Guid>>> MarkInProgress(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new MarkIncidentInProgressCommand(id), cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Responder")]
        [HttpPost("{id:guid}/resolve")]
        [SwaggerOperation(Summary = "Resolve an incident")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<Guid>>> Resolve(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ResolveIncidentCommand(id), cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Responder")]
        [HttpPost("{id:guid}/escalate")]
        [SwaggerOperation(Summary = "Escalate an incident to higher priority")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<Guid>>> Escalate(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new EscalateIncidentCommand(id), cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id:guid}/cancel")]
        [SwaggerOperation(Summary = "Cancel an incident")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<Guid>>> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CancelIncidentCommand(id), cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
