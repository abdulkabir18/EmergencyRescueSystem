using Application.Interfaces.CurrentUser;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Services.CurrentUser
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId =>
            Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid id) ? id : Guid.Empty;

        public UserRole Role =>
           Enum.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value, out UserRole role) ? role : UserRole.Unknown;

        public string Email =>
            _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        //public string? IpAddress =>
        //    _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        //public string? UserAgent =>
        //    _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
    }
}
