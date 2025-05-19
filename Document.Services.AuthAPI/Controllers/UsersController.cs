using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Document.Services.AuthAPI.Models;
using Document.Services.AuthAPI.Models.DTOs;
using Document.Services.AuthAPI.Services.IServices;
using System.Security.Claims; // For ClaimTypes

namespace Document.Services.AuthAPI.Controllers;

[ApiController]
[Route("auth-service/api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    public record UserRoleUpdateRequest([FromBody] Role NewRole);

    [HttpPatch("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UserRoleUpdateRequest request)
    {
        // Prevent admin from accidentally demoting themselves or changing super admin if such concept exists
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (id == currentUserId && request.NewRole != Role.Admin) {
             // return BadRequest("Admin cannot demote themselves from Admin role.");
        }

        var success = await _userService.UpdateUserRoleAsync(id, request.NewRole);
        if (!success)
        {
            return NotFound(new { Message = "User not found or update failed." });
        }
        return Ok(new { Message = "User role updated successfully." });
    }
}