using Document.Services.AuthAPI.Data;
using Document.Services.AuthAPI.Models;
using Document.Services.AuthAPI.Models.DTOs;
using Document.Services.AuthAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using Document.Services.AuthAPI.Service.IServices;
namespace Document.Services.AuthAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext context, PasswordHasher passwordHasher, ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<(User user, string message)> RegisterAsync(RegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
        {
            return (null, "Username already exists.");
        }

        var user = new User
        {
            Username = registerDto.Username,
            PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
            UserRole = registerDto.UserRole // Be careful with allowing direct role assignment on registration
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return (user, "User registered successfully.");
    }

    public async Task<(User user, TokenDto token, string message)> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == loginDto.Username);

        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, loginDto.Password))
        {
            return (null, null, "Invalid username or password.");
        }

        var token = _tokenService.GenerateToken(user);
        return (user, token, "Login successful.");
    }
}