using Document.Services.AuthAPI.Models;
using Document.Services.AuthAPI.Models.DTOs;

namespace Document.Services.AuthAPI.Services.IServices;
 public interface IUserService
{
    Task<IEnumerable<UserInfoDto>> GetAllUsersAsync();
    Task<UserInfoDto> GetUserByIdAsync(Guid userId);
    Task<bool> UpdateUserRoleAsync(Guid userId, Role newRole);
}