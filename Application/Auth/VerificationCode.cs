using Domain.Common.Exceptions;
using System.Security.Cryptography;

namespace Application.Auth
{
    public record VerificationCode
    {
        public Guid UserId { get; }
        public string Code { get; }
        public DateTime ExpiresAt { get; }
        public bool IsUsed { get; private set; }

        private VerificationCode(Guid userId, string code, DateTime expiresAt)
        {
            UserId = userId;
            Code = code ?? throw new ArgumentNullException(nameof(code));
            ExpiresAt = expiresAt;
            IsUsed = false;
        }

        public static VerificationCode Create(Guid userId, int expiryMinutes = 15)
        {
            string code = GenerateRandomCode();
            return new VerificationCode(userId, code, DateTime.UtcNow.AddMinutes(expiryMinutes));
        }

        public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

        public void MarkAsUsed()
        {
            if (IsExpired())
                throw new ValidationException("Verification code has expired.");
            IsUsed = true;
        }

        private static string GenerateRandomCode(int length = 6)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var digits = bytes.Select(b => (b % 10).ToString());
            return string.Concat(digits);
        }
    }
}
