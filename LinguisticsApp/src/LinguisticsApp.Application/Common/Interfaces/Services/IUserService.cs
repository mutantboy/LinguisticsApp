using LinguisticsApp.Application.Common.Commands.UserCommands;
using LinguisticsApp.Application.Common.DTOs.AuthDTOs;
using LinguisticsApp.Application.Common.DTOs.UserDTOs;
using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Interfaces.Services
{
    public interface IUserService
    {
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
        Task<bool> ChangePasswordAsync(UserId userId, UpdatePasswordCommand command);
        Task<UserDto> GetByIdAsync(UserId userId);
    }
}
