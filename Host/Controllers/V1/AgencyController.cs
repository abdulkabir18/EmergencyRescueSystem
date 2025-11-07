using Application.Common.Dtos;
using Application.Features.Agencies.Commands.AddSupportedIncident;
using Application.Features.Agencies.Commands.RemoveSupportedIncident;
using Application.Features.Agencies.Dtos;
using Application.Features.Agencies.Queries.GetAllAgencies;
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
        public async Task<ActionResult<Result<Unit>>> AddSupportedIncident(Guid agencyId, [FromBody] IncidentTypeDto typeDto)
        {
            var model = new AddSupportedIncidentRequestModel(agencyId, typeDto);
            var command = new AddSupportedIncidentCommand(model);

            var result = await _mediator.Send(command);

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
        public async Task<ActionResult<Result<Guid>>> RemoveSupportedIncident(Guid agencyId, [FromBody] IncidentTypeDto typeDto)
        {
            var model = new RemoveSupportedIncidentRequestModel(agencyId, typeDto);
            var command = new RemoveSupportedIncidentCommand(model);

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        //[Authorize(Roles = "SuperAdmin, AgencyAdmin")]
        [HttpGet("all")]
        [SwaggerOperation(
            Summary = "Get all agencies (paginated)"
        )]
        [ProducesResponseType(typeof(Result<PaginatedResult<AgencyDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<PaginatedResult<AgencyDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<PaginatedResult<AgencyDto>>>> GetAllAgencies([FromQuery] GetAllAgenciesRequest request)
        {
            var query = new GetAllAgenciesQuery(request);
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
