using System.ComponentModel.DataAnnotations;
using Document.Services.AuthAPI.Models;

namespace Document.Services.AuthAPI.Models.DTOs;

public record RegisterDto([Required] string Username, [Required] string Password, Role UserRole = Role.Viewer);
public record LoginDto([Required] string Username, [Required] string Password);
public record TokenDto(string AccessToken, DateTime ExpiresAt, string Username, string Role);
public record UserInfoDto(Guid Id, string Username, string Role);
public record LoginResponseDto(UserInfoDto User, string Token);
 