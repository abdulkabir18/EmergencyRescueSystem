using Application.Common.Dtos;
using Application.Features.Responders.Commands.UpdateResponderLocation;
using Application.Features.Responders.Commands.UpdateResponderStatus;
using Application.Features.Responders.Queries.GetAllResponders;
using Application.Features.Responders.Queries.GetCurrentUserResponder;
using Application.Features.Responders.Queries.GetNearbyResponders;
using Application.Features.Responders.Queries.GetRespondersByAgency;
using Application.Features.Responders.Queries.GetRespondersByIncident;
using Application.Features.Responders.Queries.GetResponderById;
using Application.Features.Responders.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

        [HttpGet("all")]
        [SwaggerOperation(Summary = "Get all responders (paginated)")]
        [ProducesResponseType(typeof(PaginatedResult<ResponderDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<ResponderDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAllRespondersQuery(pageNumber, pageSize), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get responder by ID")]
        [ProducesResponseType(typeof(Result<ResponderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<ResponderDto>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetResponderByIdQuery(id), cancellationToken);
            if (!result.Succeeded) return NotFound(result);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation(Summary = "Get current user's responder profile")]
        [ProducesResponseType(typeof(Result<ResponderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<ResponderDto>>> GetMine(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCurrentUserResponderQuery(), cancellationToken);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("agency/{agencyId:guid}")]
        [SwaggerOperation(Summary = "Get responders for an agency (paginated)")]
        [ProducesResponseType(typeof(PaginatedResult<ResponderDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<ResponderDto>>> GetByAgency(Guid agencyId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetRespondersByAgencyQuery(agencyId, pageNumber, pageSize), cancellationToken);
            return Ok(result);
        }

        [HttpGet("incident/{incidentId:guid}/responders")]
        [SwaggerOperation(Summary = "Get responders assigned to an incident")]
        [ProducesResponseType(typeof(Result<List<ResponderDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<List<ResponderDto>>>> GetByIncident(Guid incidentId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetRespondersByIncidentQuery(incidentId), cancellationToken);
            return Ok(result);
        }

        [HttpGet("nearby")]
        [SwaggerOperation(Summary = "Get nearby responders (paginated)")]
        [ProducesResponseType(typeof(PaginatedResult<ResponderDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<ResponderDto>>> GetNearby([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 5.0, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = new GetNearbyRespondersQuery(latitude, longitude, radiusKm, pageNumber, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPatch("{id:guid}/location")]
        [SwaggerOperation(Summary = "Update responder location")]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<Unit>>> UpdateLocation(Guid id, [FromBody] UpdateResponderLocationRequestModel model, CancellationToken cancellationToken)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(Result<Unit>.Failure("Invalid location data."));

            var result = await _mediator.Send(new UpdateResponderLocationCommand(id, model), cancellationToken);
            if (!result.Succeeded)
            {
                if (result.Message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) == true)
                    return Unauthorized(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPatch("{id:guid}/status")]
        [SwaggerOperation(Summary = "Update responder status")]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<Unit>>> UpdateStatus(Guid id, [FromBody] UpdateResponderStatusRequestModel model, CancellationToken cancellationToken)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(Result<Unit>.Failure("Invalid status data."));

            var result = await _mediator.Send(new UpdateResponderStatusCommand(id, model), cancellationToken);
            if (!result.Succeeded)
            {
                if (result.Message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) == true)
                    return Unauthorized(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
