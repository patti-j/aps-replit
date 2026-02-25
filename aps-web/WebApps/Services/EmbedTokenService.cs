using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ReportsWebApp.Services
{
    public class EmbedTokenService : IEmbedTokenService
    {
        private readonly byte[] _signingKey;

        public EmbedTokenService(IConfiguration configuration)
        {
            var secret = configuration["EMBED_TOKEN_SECRET"]
                         ?? Environment.GetEnvironmentVariable("EMBED_TOKEN_SECRET")
                         ?? throw new InvalidOperationException("EMBED_TOKEN_SECRET is not configured.");
            _signingKey = Encoding.UTF8.GetBytes(secret);
        }

        public string GenerateEmbedToken(string email, int companyId, bool hasAIAnalyticsRole, bool isCompanyAdmin)
        {
            var now = DateTime.UtcNow;
            var claims = new[]
            {
                new Claim("email", email),
                new Claim("companyId", companyId.ToString()),
                new Claim("hasAIAnalyticsRole", hasAIAnalyticsRole.ToString().ToLowerInvariant()),
                new Claim("isCompanyAdmin", isCompanyAdmin.ToString().ToLowerInvariant()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(_signingKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "PlanetTogether.WebApp",
                audience: "PlanetTogether.EmbedApp",
                claims: claims,
                notBefore: now.AddSeconds(-30),
                expires: now.AddMinutes(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}