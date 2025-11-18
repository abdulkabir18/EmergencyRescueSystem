using Application.Common.Dtos;
using Application.Features.Agencies.Commands.AddSupportedIncident;
using Application.Features.Agencies.Commands.RemoveSupportedIncident;
using Application.Features.Agencies.Commands.SetLogo;
using Application.Features.Agencies.Commands.UpdateAgencyAddress;
using Application.Features.Agencies.Dtos;
using Application.Features.Agencies.Queries.GetAgenciesByIncidentType;
using Application.Features.Agencies.Queries.GetAgencyById;
using Application.Features.Agencies.Queries.GetAllAgencies;
using Application.Features.Agencies.Queries.GetSupportedIncidents;
using Application.Features.Agencies.Queries.SearchAgencies;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Host.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AgencyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AgencyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "SuperAdmin, AgencyAdmin")]
        [HttpPost("{agencyId:guid}/add-supported-incident")]
        [SwaggerOperation(
            Summary = "Add supported incident to an agency",
            Description = "Allows SuperAdmin or AgencyAdmin to add incident type to an agency."
        )]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<Unit>>> AddSupportedIncident(Guid agencyId, [FromBody] IncidentTypeDto typeDto, CancellationToken cancellationToken)
        {
            var model = new AddSupportedIncidentRequestModel(agencyId, typeDto);
            var command = new AddSupportedIncidentCommand(model);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "SuperAdmin, AgencyAdmin")]
        [HttpDelete("{agencyId:guid}/remove-supported-incident")]
        [SwaggerOperation(
            Summary = "Remove supported incident from an agency",
            Description = "Allows SuperAdmin or AgencyAdmin to remove a supported incident type from an agency."
        )]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<Guid>>> RemoveSupportedIncident(Guid agencyId, [FromBody] IncidentTypeDto typeDto, CancellationToken cancellationToken)
        {
            var model = new RemoveSupportedIncidentRequestModel(agencyId, typeDto);
            var command = new RemoveSupportedIncidentCommand(model);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("all")]
        [SwaggerOperation(
            Summary = "Get all agencies (paginated)"
        )]
        [ProducesResponseType(typeof(PaginatedResult<AgencyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<AgencyDto>>> GetAllAgencies([FromQuery] GetAllAgenciesRequest request, CancellationToken cancellationToken)
        {
            var query = new GetAllAgenciesQuery(request);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get agency by ID",
            Description = "Retrieves the details of a specific agency by its unique identifier."
        )]
        [ProducesResponseType(typeof(Result<AgencyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<AgencyDto>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAgencyByIdQuery(id), cancellationToken);

            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("by-incident-type")]
        [SwaggerOperation(
            Summary = "Get all agencies that support a given incident type (paginated)"
        )]
        [ProducesResponseType(typeof(PaginatedResult<AgencyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<AgencyDto>>> GetByIncidentType([FromQuery] IncidentType type, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = new GetAgenciesByIncidentTypeQuery(type, pageNumber, pageSize);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Search agencies (paginated)"
        )]
        [ProducesResponseType(typeof(PaginatedResult<AgencyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<AgencyDto>>> Search([FromQuery] string keyword, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = new SearchAgenciesQuery(keyword ?? string.Empty, pageNumber, pageSize);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}/supported-incidents")]
        [SwaggerOperation(
            Summary = "Get supported incident types for an agency"
        )]
        [ProducesResponseType(typeof(Result<List<IncidentType>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<List<IncidentType>>>> GetSupportedIncidents(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAgencySupportedIncidentsQuery(id), cancellationToken);

            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPatch("logo")]
        [SwaggerOperation(
            Summary = "Set or update the agency's logo.",
            Description = "Allows an authenticated agency admin to set or update their agency's logo image."
        )]
        [Authorize(Roles = "AgencyAdmin")]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<Unit>>> SetLogo([FromForm] SetLogoCommand command, CancellationToken cancellationToken)
        {
            if (command == null || command.Logo == null)
            {
                return BadRequest(Result<Unit>.Failure("Invalid logo file provided."));
            }
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPatch("update-address")]
        [SwaggerOperation(
            Summary = "Update the agency's address.",
            Description = "Allows an authenticated agency admin to update their agency's address details."
        )]
        [Authorize(Roles = "AgencyAdmin")]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<Unit>>> UpdateAddress([FromBody] UpdateAddressCommand command, CancellationToken cancellationToken)
        {
            if (command == null || command.Address == null)
            {
                return BadRequest(Result<Unit>.Failure("Invalid address details provided."));
            }
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
