using Microsoft.AspNetCore.Mvc;
using Document.Services.AuthAPI.Models.DTOs;
using Document.Services.AuthAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Document.Services.AuthAPI.Models;
namespace Document.Services.AuthAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        // restrict who can register or what roles they can self-assign.
        if (registerDto.UserRole == Role.Admin && !User.IsInRole(Role.Admin.ToString())) 
        {
            return Forbid("Only admins can create other admin users during registration.");
        }

        var (user, message) = await _authService.RegisterAsync(registerDto);
        if (user == null)
        {
            return BadRequest(new { Message = message });
        }
        return Ok(new { Message = message, UserId = user.Id });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var (user, token, message) = await _authService.LoginAsync(loginDto);
        if (user == null || token == null)
        {
            return Unauthorized(new { Message = message });
        }
        return Ok(token);
    }

    [HttpPost("logout")]
    [Authorize] 
    public IActionResult Logout()
    {
        return Ok(new { Message = "Logout successful. Please discard your token." });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // or JwtRegisteredClaimNames.Sub
        var userName = User.FindFirstValue(ClaimTypes.Name); // or JwtRegisteredClaimNames.UniqueName
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        return Ok(new { Id = userId, Username = userName, Role = userRole });
    }
}