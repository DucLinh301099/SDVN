using Microsoft.AspNetCore.Mvc;
using SDVN.Models.ViewModel;
using SDVN.Services;
using System.Threading.Tasks;

namespace SDVN.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Đăng ký người dùng mới
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestVM request)
        {
            var result = await _authService.RegisterAsync(request);
            if (result == -1) return BadRequest("Phone number already exists");

            return Ok(new { message = "User registered successfully", userId = result });
        }

        // Đăng nhập và nhận token JWT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestVM request)
        {
            var authResult = await _authService.AuthenticateUserAsync(request);
            if (authResult == null) return Unauthorized("Invalid credentials");

            return Ok(new
            {
                token = authResult.Token,
                role = authResult.Role
            });
        }
    }
}
