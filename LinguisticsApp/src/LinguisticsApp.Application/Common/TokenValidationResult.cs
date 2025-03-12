using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common
{
    public class TokenValidationResult
    {
        public bool IsValid { get; }
        public string? ErrorMessage { get; }
        public UserId? UserId { get; }
        public UserRole Role { get; }

        private TokenValidationResult(bool isValid, string? errorMessage, UserId? userId, UserRole role)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            UserId = userId;
            Role = role;
        }

        public static TokenValidationResult Success(UserId userId, UserRole role)
        {
            return new TokenValidationResult(true, null, userId, role);
        }

        public static TokenValidationResult Failed(string errorMessage)
        {
            return new TokenValidationResult(false, errorMessage, null, UserRole.None);
        }
    }

    public enum UserRole
    {
        None,
        Researcher,
        Admin
    }
}
