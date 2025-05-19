using Document.Services.AuthAPI.Models.DTOs;
using Document.Services.AuthAPI.Models;
namespace Document.Services.AuthAPI.Services.IServices;

public interface IAuthService
{
    Task<(User user, string message)> RegisterAsync(RegisterDto registrationRequestDto);
    Task<(User user, TokenDto token, string message)> LoginAsync(LoginDto loginRequestDto);

}

