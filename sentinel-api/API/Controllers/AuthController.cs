using Microsoft.AspNetCore.Mvc;
using sentinel_api.Application.DTOs;
using sentinel_api.Core.Interfaces;

namespace sentinel_api.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            return Ok(result);
        }

        [HttpGet("confirmUserEmail")]
        public async Task<IActionResult> ConfirmUserEmail(Guid id)
        {
            var result = await _authService.ConfirmUserEmailAsync(id);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            return Ok(result);
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto);

            return Ok(result);
        }

        [HttpGet("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromQuery] string email, [FromQuery] string token)
        {
            var dto = new ResetPasswordDto();
            dto.Email = email;
            dto.Token = token;
            var result = await _authService.ResetPasswordAsync(dto);

            return Ok(result);
        }
    }
}
