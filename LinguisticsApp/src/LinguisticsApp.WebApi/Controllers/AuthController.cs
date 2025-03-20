using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinguisticsApp.Application.Common.Interfaces.Services;
using LinguisticsApp.Application.Common.Commands.UserCommands;
using LinguisticsApp.Application.Common.DTOs.AuthDTOs;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.Infrastructure.Service.User;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace LinguisticsApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.LoginAsync(model);

                if (result == null)
                    return BadRequest(new { message = "Invalid email or password" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.RegisterAsync(model);

                if (result == null)
                    return BadRequest(new { message = "Email already registered or invalid data provided" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordCommand model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                 User.FindFirst(ClaimTypes.NameIdentifier) ??
                                 User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase));

                if (userIdClaim == null)
                {
                    var allClaims = string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}"));
                    return Unauthorized(new { message = "User ID claim not found", availableClaims = allClaims });
                }

                if (!Guid.TryParse(userIdClaim.Value, out var userIdGuid))
                {
                    return Unauthorized(new { message = "Invalid user ID format", value = userIdClaim.Value });
                }

                var userId = new UserId(userIdGuid);
                var result = await _userService.ChangePasswordAsync(userId, model);

                if (!result)
                    return BadRequest(new { message = "Current password is incorrect or user not found" });

                return Ok(new { message = "Password updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}