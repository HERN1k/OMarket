using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OMarket.Domain.DTOs;
using OMarket.Domain.Interfaces.Application.Services.Admin;

namespace OMarket.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AuthController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken token)
        {
            await _adminService.RegisterAsync(request, token);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken token)
        {
            LoginResponse response = await _adminService.LoginAsync(request, HttpContext, token);

            return Ok(new { Data = response });
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout(CancellationToken token)
        {
            await _adminService.LogoutAsync(HttpContext, token);

            return Ok();
        }

        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshToken(CancellationToken token)
        {
            await _adminService.RefreshTokenAsync(HttpContext, token);

            return Ok();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken token)
        {
            await _adminService.ChangePasswordAsync(request, HttpContext, token);

            return Ok();
        }

        [Authorize(Policy = "RequireAdminOrSuperAdminRole")]
        [HttpPost("remove-admin")]
        public async Task<IActionResult> RemoveAdmin([FromBody] RemoveAdminRequest request, CancellationToken token)
        {
            await _adminService.RemoveAdminAsync(request, HttpContext, token);

            return Ok();
        }

        [Authorize(Policy = "RequireAdminOrSuperAdminRole")]
        [HttpGet("test")]
        public IActionResult Test()
        {
            var claims = HttpContext.User.Claims;

            foreach (var claim in claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            return Ok(new { Text = "Nice!" });
        }
    }
}