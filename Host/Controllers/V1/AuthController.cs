using Application.Common.Dtos;
using Application.Features.Agencies.Commands.RegisterAgency;
using Application.Features.Agencies.Dtos;
using Application.Features.Auth.Commands.ConfirmForgotPassword;
using Application.Features.Auth.Commands.ContinueWithGoogle;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.ResendVerificationCode;
using Application.Features.Auth.Commands.VerifyUserEmail;
using Application.Features.Auth.Dtos;
using Application.Features.Responders.Commands.RegisterResponder;
using Application.Features.Responders.Dtos;
using Application.Features.Users.Commands.ReactivateOrDeactivate;
using Application.Features.Users.Commands.RegisterUser;
using Application.Features.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Host.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("signup")]
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Creates a new user account in the system."
        )]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<Guid>>> Signup([FromForm] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("register-agency")]
        [SwaggerOperation(
            Summary = "Register a new agency and its admin user.",
            Description = "Creates an agency, uploads logo, assigns an agency admin, and registers supported incidents."
        )]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<Guid>>> RegisterAgency([FromForm] RegisterAgencyFullRequestModel model)
        {
            var typeDto = new IncidentTypesDto
            {
                SupportedIncidents = model.IncidentTypesEnums.Select(i => new IncidentTypeDto
                { AcceptedIncidentType = i }).ToList()
            };

            var commandModel = new RegisterAgencyRequestModel(
                model.RegisterUserRequest,
                model.AgencyName,
                model.AgencyEmail,
                model.AgencyPhoneNumber,
                model.AgencyLogo,
                model.AgencyAddress
            );

            var command = new RegisterAgencyCommand(commandModel, typeDto);
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }


        [Authorize(Roles = "SuperAdmin, AgencyAdmin")]
        [HttpPost("register-responder")]
        [SwaggerOperation(
            Summary = "Register a new responder",
            Description = "Allows a SuperAdmin or AgencyAdmin to register a new responder."
        )]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<Guid>>> RegisterResponder([FromForm] RegisterResponderRequestModel model)
        {
            var command = new RegisterResponderCommand(model);
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "User login",
            Description = "Authenticates a user and returns a JWT token."
        )]
        [ProducesResponseType(typeof(Result<LoginResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<LoginResponseModel>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<LoginResponseModel>>> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("continue-with-google")]
        [SwaggerOperation(Summary = "Continue login or register with Google account")]
        [ProducesResponseType(typeof(Result<LoginResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<LoginResponseModel>>> ContinueWithGoogle([FromBody] GoogleLoginRequestModel model)
        {
            var result = await _mediator.Send(new ContinueWithGoogleCommand(model));

            if (!result.Succeeded)
                return BadRequest(result); 

            return Ok(result);
        }

        [HttpPost("verify-email")]
        [SwaggerOperation(
            Summary = "Verify user email",
            Description = "Verifies a user's email address using a verification code."
        )]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<bool>>> VerifyEmail([FromBody] VerifyUserEmailCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        [SwaggerOperation(
            Summary = "Forgot password",
            Description = "Sends a password reset verification code to the user's registered email address."
        )]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<bool>>> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("confirm-forgot-password")]
        [SwaggerOperation(
            Summary = "Confirm forgot password (token/OTP)",
            Description = "Resets a user's password using a verification code (OTP) sent to their email. Provide Email, Code and the new password."
        )]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Result<bool>>> ConfirmForgotPassword([FromBody] ConfirmForgotPasswordCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPatch("{id:guid}/status")]
        [SwaggerOperation(
            Summary = "Reactivate or Deactivate a User",
            Description = "Allows a SuperAdmin to toggle a user's active status. " +
                          "If the user is active, they will be deactivated. If inactive, they will be reactivated."
        )]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Unit>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Result<Unit>>> ReactivateOrDeactivateUser(Guid id)
        {
            var command = new ReactivateOrDeactivateCommand(new ReactivateOrDeactivateRequestModel(id));
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}