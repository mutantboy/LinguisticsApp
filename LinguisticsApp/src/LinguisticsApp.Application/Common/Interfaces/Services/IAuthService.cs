using LinguisticsApp.DomainModel.DomainModel;
using System.Security.Claims;

namespace LinguisticsApp.Application.Common.Interfaces.Services
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
        string GenerateToken(User user);
        Task<TokenValidationResult> ValidateTokenAsync(string token);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}
