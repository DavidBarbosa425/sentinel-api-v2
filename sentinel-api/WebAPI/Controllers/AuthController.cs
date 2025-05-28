using Microsoft.AspNetCore.Http;
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
            var (succeeded, errors) = await _authService.RegisterAsync(dto);

            if (!succeeded)
                return BadRequest(errors);

            return Ok("Usuário registrado com sucesso");
        }
    }
}
