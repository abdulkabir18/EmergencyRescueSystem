using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Host.Controllers.V1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IInAppNotificationService _notificationService;

        public NotificationController(IInAppNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("send")]
        [Authorize(Roles = "SuperAdmin,AgencyAdmin")]
        [SwaggerOperation(
            Summary = "Send a notification to a specific user",
            Description = "Allows a SuperAdmin or AgencyAdmin to send a notification to a single user."
        )]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Result<string>>> SendToUserAsync([FromQuery] Guid recipientId, [FromQuery] string title, [FromQuery] string message, [FromQuery] NotificationType type, [FromQuery] Guid? targetId = null, [FromQuery] string? targetType = null)
        {
            await _notificationService.SendToUserAsync(recipientId, title, message, type, targetId, targetType);
            return Ok(Result<string>.Success("Notification sent successfully."));
        }

        [HttpPost("broadcast")]
        [Authorize(Roles = "SuperAdmin,AgencyAdmin")]
        [SwaggerOperation(
            Summary = "Broadcast a notification to multiple users",
            Description = "Allows a SuperAdmin or AgencyAdmin to send the same notification to multiple users at once."
        )]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Result<string>>> BroadcastAsync([FromBody] IEnumerable<Guid> recipientIds, [FromQuery] string title, [FromQuery] string message, [FromQuery] NotificationType type, [FromQuery] Guid? targetId = null, [FromQuery] string? targetType = null)
        {
            await _notificationService.BroadcastAsync(recipientIds, title, message, type, targetId, targetType);
            return Ok(Result<string>.Success("Broadcast notification sent successfully."));
        }

        [HttpPatch("{notificationId:guid}/read")]
        [SwaggerOperation(
            Summary = "Mark a notification as read",
            Description = "Marks the specified notification as read for the current user."
        )]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<string>>> MarkAsReadAsync(Guid notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok(Result<string>.Success("Notification marked as read."));
        }
    }
}