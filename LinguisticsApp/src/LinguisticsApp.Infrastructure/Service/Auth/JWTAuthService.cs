using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using LinguisticsApp.Application.Common.Interfaces.Services;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;
using CustomTokenResult = LinguisticsApp.Application.Common.TokenValidationResult;
using LinguisticsApp.Infrastructure.Service.Auth;
using LinguisticsApp.Application.Common;

namespace LinguisticsApp.Infrastructure.Auth
{
    public class JwtAuthService : IAuthService
    {
        private readonly JwtAuthOptions _options;
        private readonly byte[] _secretKeyBytes;
        private readonly SigningCredentials _signingCredentials;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JwtAuthService(IOptions<JwtAuthOptions> options)
        {
            _options = options.Value;
            _secretKeyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);

            var securityKey = new SymmetricSecurityKey(_secretKeyBytes);
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrEmpty(_options.Issuer),
                ValidateAudience = !string.IsNullOrEmpty(_options.Audience),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.Zero
            };
        }

        public string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            //PBKDF2
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt;
            try
            {
                salt = Convert.FromBase64String(parts[0]);
            }
            catch
            {
                return false;
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: providedPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return parts[1] == hashed;
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Email, user.Username.Value),
                new Claim(ClaimTypes.Role, user is Admin ? "Admin" : "Researcher"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.TokenExpirationMinutes),
                signingCredentials: _signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<CustomTokenResult> ValidateTokenAsync(string token)
        {
            var principal = GetPrincipalFromToken(token);
            if (principal == null)
                return CustomTokenResult.Failed("Invalid token");

            var expClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp);
            if (expClaim != null)
            {
                var expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
                if (expiration.UtcDateTime <= DateTime.UtcNow)
                    return CustomTokenResult.Failed("Token is expired");
            }

            var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userId))
                return CustomTokenResult.Failed("Invalid user ID");

            var roleClaim = principal.FindFirst(ClaimTypes.Role);
            var role = roleClaim?.Value == "Admin" ? UserRole.Admin : UserRole.Researcher;

            return CustomTokenResult.Success(new UserId(userId), role);
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return validatedToken is JwtSecurityToken jwtSecurityToken &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature,
                       StringComparison.InvariantCultureIgnoreCase);
        }
    }
}