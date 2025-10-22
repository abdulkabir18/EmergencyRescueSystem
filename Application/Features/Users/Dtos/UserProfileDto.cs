namespace Application.Features.Users.Dtos
{
    public record UserProfileDto
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = default!;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string Gender { get; set; } = default!;
        public string? ProfilePictureUrl { get; set; }
    }
}
