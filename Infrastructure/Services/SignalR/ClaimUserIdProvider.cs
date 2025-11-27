using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Infrastructure.Services.SignalR
{
    public class ClaimUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            if (connection?.User == null)
                return null;

            var userId =
                connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                connection.User.FindFirst("sub")?.Value ??
                connection.User.FindFirst("id")?.Value ??
                connection.User.FindFirst("userId")?.Value;

            return userId;
        }
    }
}