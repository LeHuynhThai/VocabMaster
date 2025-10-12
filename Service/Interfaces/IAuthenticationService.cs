using Repository.DTOs;
using Repository.Entities;
using System.Security.Claims;

namespace Service.Interfaces
{
    public interface IAuthenticationService
    {
        Task<TokenResponseDto> Login(string name, string password);
        Task<bool> Register(User user);
        Task Logout();
        Task<User> GetCurrentUser();
        Task<TokenResponseDto> AuthenticateGoogleUser(GoogleAuthDto googleAuth);
        Task<GoogleUserInfoDto> GetGoogleUserInfo(string accessToken);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        Task<TokenResponseDto> GenerateJwtToken(User user);
        List<Claim> CreateUserClaims(User user);
        string FindUserIdFromClaims(ClaimsPrincipal user);
    }
}