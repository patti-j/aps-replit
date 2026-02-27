using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.IdentityModel.Tokens;

namespace ReportsWebApp.Services
{
    public class EmbedTokenService : IEmbedTokenService
    {
        private readonly byte[] _signingKey;

        public EmbedTokenService(IConfiguration configuration)
        {
            var secret = GetSecretFromKeyVault(configuration, "EMBED-TOKEN-SECRET")
                         ?? Environment.GetEnvironmentVariable("EMBED_TOKEN_SECRET");

            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("EMBED_TOKEN_SECRET could not be retrieved from Azure Key Vault. Ensure the secret EMBED-TOKEN-SECRET exists in the Key Vault.");
            }

            _signingKey = Encoding.UTF8.GetBytes(secret);
        }

        private static string? GetSecretFromKeyVault(IConfiguration configuration, string secretName)
        {
            try
            {
                string kvUrl = configuration["KeyValultUrl"];
                string tenantId = configuration["TenantId"];
                string clientId = configuration["ClientId"];
                string clientSecret = configuration["ClientSecret"];

                if (string.IsNullOrEmpty(kvUrl) || string.IsNullOrEmpty(tenantId) ||
                    string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return null;
                }

                var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                var client = new SecretClient(new Uri(kvUrl), credential);
                KeyVaultSecret secret = client.GetSecret(secretName);
                return secret.Value;
            }
            catch
            {
                return null;
            }
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