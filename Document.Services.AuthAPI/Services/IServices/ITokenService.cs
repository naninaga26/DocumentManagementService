using Document.Services.AuthAPI.Models;
using Document.Services.AuthAPI.Models.DTOs;

namespace Document.Services.AuthAPI.Services.IServices;

public interface ITokenService
{
    TokenDto GenerateToken(User appUser);
}

