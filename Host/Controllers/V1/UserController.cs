using Application.Common.Dtos;
using Application.Features.Users.Commands.ResetPassword;
using Application.Features.Users.Commands.SetProfileImage;
using Application.Features.Users.Commands.UpdateFullName;
using Application.Features.Users.Dtos;
using Application.Features.Users.Queries.GetAllUser;
using Application.Features.Users.Queries.GetAllUserByRole;
using Application.Features.Users.Queries.GetProfile;
using Application.Features.Users.Queries.GetTotalUserCount;
using Application.Features.Users.Queries.GetUserByEmail;
using Application.Features.Users.Queries.GetUserById;
using Application.Features.Users.Queries.SearchUsers;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(Result<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UserProfileDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<UserProfileDto>>> GetProfile()
        {
            var result = await _mediator.Send(new GetProfileQuery());

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [SwaggerOperation(
            Summary = "Upload or update the current user's profile image.",
            Description = "Requires authentication. The request must be `multipart/form-data` and include an image file under the 'image' key."
        )]
        [Authorize]
        [HttpPatch("profile-image")]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<Unit>>> SetProfileImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest(Result<Unit>.Failure("An image file is required."));
            }

            var result = await _mediator.Send(new SetProfileImageCommand(image));

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [SwaggerOperation(
            Summary = "Update the current user's details.",
            Description = "Requires authentication. Update fields like first name and last name."
        )]
        [Authorize]
        [HttpPatch("details")]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<Unit>>> UpdateBasicDetails([FromBody] UpdateFullNameCommand command)
        {
            if (command == null || command.Model == null)
            {
                return BadRequest(Result<Unit>.Failure("Invalid user details provided."));
            }

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [SwaggerOperation(
            Summary = "Update the current user's address.",
            Description = "Requires authentication. Update fields like street, city, state, and zip code."
        )] 
        [Authorize]
        [HttpPatch("update-address")]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<Unit>>> UpdateAddress([FromBody] UpdateFullNameCommand command)
        {
            if (command == null || command.Model == null)
            {
                return BadRequest(Result<Unit>.Failure("Invalid user details provided."));
            }
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return Ok(result);
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

        //[Authorize(Roles = "SuperAdmin")]
        [HttpGet("total-count")]
        [SwaggerOperation(Summary = "Get total user count")]
        public async Task<ActionResult<Result<int>>> GetTotalUserCount()
        {
            var result = await _mediator.Send(new GetTotalUserCountQuery());
            return Ok(result);
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

        //[Authorize(Roles = "SuperAdmin")]
        [HttpGet("all")]
        [SwaggerOperation(Summary = "Get all users (paginated)")]
        [ProducesResponseType(typeof(PaginatedResult<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<UserDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetAllUserQuery(pageNumber, pageSize));
            return Ok(result);
        }

        [SwaggerOperation(
            Summary = "Reset the current user's password.",
            Description = "Requires authentication. Provide current password, new password, and confirmation of the new password."
        )]
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<bool>>> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            if (command == null || command.Model == null)
            {
                return BadRequest(Result<bool>.Failure("Invalid password reset data."));
            }

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
