using Domain.Enums;

namespace Application.Interfaces.CurrentUser
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        UserRole Role { get; }
        string Email { get; }
        //string? IpAddress { get; }
        //string? UserAgent { get; }
    }
}
