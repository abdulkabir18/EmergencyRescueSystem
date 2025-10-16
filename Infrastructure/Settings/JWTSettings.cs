namespace Infrastructure.Settings
{
    public class JWTSettings
    {
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public int ExpiryInMinutes { get; set; } = default!;
    }
}
