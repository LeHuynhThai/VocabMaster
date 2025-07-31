using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services;

public interface IAccountService
{
    Task<TokenResponseDto> Login(string name, string password);
    Task<bool> Register(User user);
    string HashPassword(string password);
    Task Logout();
    Task<User> GetCurrentUser();
    Task<TokenResponseDto> GenerateJwtToken(User user); // Create JWT token
    Task<TokenResponseDto> AuthenticateGoogleUser(GoogleAuthDto googleAuth); // Authenticate Google user
    Task<GoogleUserInfoDto> GetGoogleUserInfo(string accessToken);
}
