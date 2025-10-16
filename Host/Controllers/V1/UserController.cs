using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Application.Features.Users.Queries.GetAllUserByRole;
using Application.Features.Users.Queries.GetUserByEmail;
using Application.Features.Users.Queries.GetUserById;
using Application.Features.Users.Queries.SearchUsers;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Host.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search users by keyword (name, email, etc.)")]
        [ProducesResponseType(typeof(PaginatedResult<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<UserDto>>> SearchUsers([FromQuery] string keyword, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new SearchUsersQuery(keyword, pageNumber, pageSize));
            return Ok(result);
        }

        
        [HttpGet("by-email")]
        [SwaggerOperation(Summary = "Get a user by email")]
        [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<UserDto>>> GetByEmail([FromQuery] string email)
        {
            var result = await _mediator.Send(new GetUserByEmailQuery(email));
            if (result.Succeeded) return Ok(result);
            return NotFound(result);
        }

        
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get a user by ID")]
        [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<UserDto>>> GetById([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
            if (result.Succeeded) return Ok(result);
            return NotFound(result);
        }

      
        [HttpGet("by-role")]
        [SwaggerOperation(Summary = "Get all users by role (paginated)")]
        [ProducesResponseType(typeof(PaginatedResult<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<UserDto>>> GetAllByRole([FromQuery] UserRole role, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetAllUserByRoleQuery(role, pageNumber, pageSize));
            return Ok(result);
        }
    }
}
