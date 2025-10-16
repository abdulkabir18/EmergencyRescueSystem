using Domain.Common.Security;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Security
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasherSettings _passwordHasherSettings; 

        public BcryptPasswordHasher(IOptions<PasswordHasherSettings> options)
        {
            _passwordHasherSettings = options.Value;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, _passwordHasherSettings.WorkFactor);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
    }
}
