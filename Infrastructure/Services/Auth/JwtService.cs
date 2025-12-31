using Application.Interfaces.Auth;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services.Auth
{
    public class JwtService : IAuthService
    {
        private readonly JWTSettings _jwtSettings;

        public JwtService(IOptions<JWTSettings> options)
        {
            _jwtSettings = options.Value;
        }
        public string GenerateToken(Guid userId, string email, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credential = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_jwtSettings.Issuer, _jwtSettings.Audience, claims, null, DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtSettings.ExpiryInMinutes)), credential);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}