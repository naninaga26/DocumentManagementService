using Document.Services.AuthAPI.Data;
using Document.Services.AuthAPI.Models;
using Document.Services.AuthAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Document.Services.AuthAPI.Services.IServices;

namespace Document.Services.AuthAPI.Services;
public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext context)
    {
        _db = context;
    }

    public async Task<IEnumerable<UserInfoDto>> GetAllUsersAsync()
    {
        return await _db.Users
            .Select(u => new UserInfoDto(u.Id, u.Username, u.UserRole.ToString()))
            .ToListAsync();
    }

    public async Task<UserInfoDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user == null ? null : new UserInfoDto(user.Id, user.Username, user.UserRole.ToString());
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, Role newRole)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        user.UserRole = newRole;
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return true;
    }
}