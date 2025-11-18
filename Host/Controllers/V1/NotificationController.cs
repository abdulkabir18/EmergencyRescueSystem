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
        public async Task<ActionResult<Result<string>>> SendToUser([FromQuery] Guid recipientId, [FromQuery] string title, [FromQuery] string message, [FromQuery] NotificationType type, [FromQuery] Guid? targetId = null, [FromQuery] string? targetType = null)
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
        public async Task<ActionResult<Result<string>>> Broadcast([FromBody] IEnumerable<Guid> recipientIds, [FromQuery] string title, [FromQuery] string message, [FromQuery] NotificationType type, [FromQuery] Guid? targetId = null, [FromQuery] string? targetType = null)
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
        public async Task<ActionResult<Result<string>>> MarkAsRead(Guid notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok(Result<string>.Success("Notification marked as read."));
        }

        [Authorize]
        [HttpGet("user/{userId:guid}")]
        [SwaggerOperation(
            Summary = "Get paginated notifications for a user",
            Description = "Retrieves notifications for the specified user, paginated by page number and size."
        )]
        [ProducesResponseType(typeof(PaginatedResult<NotificationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<NotificationDto>>> GetUserNotifications(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _notificationService.GetUserNotificationsAsync(userId, pageNumber, pageSize);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("user/{userId:guid}/unread-count")]
        [SwaggerOperation(
            Summary = "Get unread notification count for a user",
            Description = "Returns the total number of unread notifications for the specified user."
        )]
        [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<int>>> GetUnreadCount(Guid userId)
        {
            int count = await _notificationService.GetUnreadCountAsync(userId);
            if (count < 0)
                return BadRequest(Result<int>.Failure("User not found."));

            return Ok(Result<int>.Success(count, "Unread count retrieved successfully."));
        }
    }
}