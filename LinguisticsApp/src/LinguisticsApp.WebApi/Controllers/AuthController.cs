using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.Application.Common.Commands.UserCommands;
using LinguisticsApp.Application.Common.DTOs.AuthDTOs;
using LinguisticsApp.Application.Common.Interfaces.Services;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;

namespace LinguisticsApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;

        public AuthController(IUnitOfWork unitOfWork, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(new Email(model.Email));
                if (user == null)
                    return BadRequest(new { message = "Invalid email or password" });

                if (!_authService.VerifyPassword(user.Password, model.Password))
                    return BadRequest(new { message = "Invalid email or password" });

                var token = _authService.GenerateToken(user);

                var response = new AuthResultDto
                {
                    Token = token,
                    UserName = $"{user.FirstName} {user.LastName}",
                    Role = user is Admin ? "Admin" : "Researcher",
                    UserId = user.Id.Value
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(new Email(model.Email));
                if (existingUser != null)
                    return BadRequest(new { message = "Email already registered" });

                var hashedPassword = _authService.HashPassword(model.Password);

                User user;
                var userId = UserId.New();

                if (model.IsAdmin)
                {
                    user = new Admin(
                        userId,
                        new Email(model.Email),
                        hashedPassword,
                        model.FirstName,
                        model.LastName,
                        false /// Default value for CanModifyRules
                    );
                }
                else
                {
                    if (string.IsNullOrEmpty(model.Institution) || !model.Field.HasValue)
                        return BadRequest(new { message = "Institution and field are required for researchers" });

                    user = new Researcher(
                        userId,
                        new Email(model.Email),
                        hashedPassword,
                        model.FirstName,
                        model.LastName,
                        model.Institution,
                        model.Field.Value
                    );
                }

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var token = _authService.GenerateToken(user);

                var response = new AuthResultDto
                {
                    Token = token,
                    UserName = $"{user.FirstName} {user.LastName}",
                    Role = user is Admin ? "Admin" : "Researcher",
                    UserId = user.Id.Value
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordCommand model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userIdGuid))
                    return Unauthorized();

                var userId = new UserId(userIdGuid);

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                if (!_authService.VerifyPassword(user.Password, model.CurrentPassword))
                    return BadRequest(new { message = "Current password is incorrect" });

                var hashedPassword = _authService.HashPassword(model.NewPassword);
                user.UpdatePassword(hashedPassword);

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Password updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}