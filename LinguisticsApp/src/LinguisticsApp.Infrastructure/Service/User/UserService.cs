using AutoMapper;
using LinguisticsApp.Application.Common.Commands.UserCommands;
using LinguisticsApp.Application.Common.DTOs.AuthDTOs;
using LinguisticsApp.Application.Common.DTOs.UserDTOs;
using LinguisticsApp.Application.Common.Interfaces.Services;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Infrastructure.Service.User
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IAuthService authService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            var email = new Email(loginDto.Email);
            var user = await _unitOfWork.Users.GetByEmailAsync(email);

            if (user == null || !_authService.VerifyPassword(user.Password, loginDto.Password))
                return null; // Return null to indicate failed login

            var token = _authService.GenerateToken(user);

            return new AuthResultDto
            {
                Token = token,
                UserName = $"{user.FirstName} {user.LastName}",
                Role = user is Admin ? "Admin" : "Researcher",
                UserId = user.Id.Value
            };
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            var email = new Email(registerDto.Email);
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);

            if (existingUser != null)
                return null; // Return null to indicate user already exists

            var hashedPassword = _authService.HashPassword(registerDto.Password);

            DomainModel.DomainModel.User user;
            var userId = UserId.New();

            if (registerDto.IsAdmin)
            {
                user = new Admin(
                    userId,
                    email,
                    hashedPassword,
                    registerDto.FirstName,
                    registerDto.LastName,
                    false
                );
            }
            else
            {
                if (string.IsNullOrEmpty(registerDto.Institution) || !registerDto.Field.HasValue)
                    return null; 

                user = new Researcher(
                    userId,
                    email,
                    hashedPassword,
                    registerDto.FirstName,
                    registerDto.LastName,
                    registerDto.Institution,
                    registerDto.Field.Value
                );
            }

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var token = _authService.GenerateToken(user);

            return new AuthResultDto
            {
                Token = token,
                UserName = $"{user.FirstName} {user.LastName}",
                Role = user is Admin ? "Admin" : "Researcher",
                UserId = user.Id.Value
            };
        }

        public async Task<bool> ChangePasswordAsync(UserId userId, UpdatePasswordCommand command)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            if (!_authService.VerifyPassword(user.Password, command.CurrentPassword))
                return false;

            var hashedPassword = _authService.HashPassword(command.NewPassword);
            user.UpdatePassword(hashedPassword);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto> GetByIdAsync(UserId userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return _mapper.Map<UserDto>(user);
        }
    }
}
