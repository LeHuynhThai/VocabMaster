using Repository.Entities;
using System.Security.Claims;

namespace Service.Interfaces
{
    public interface IAuthenticationService
    {
        Task<Dictionary<string, object>?> Login(string name, string password);
        Task<bool> Register(User user);
        Task Logout();
        Task<User> GetCurrentUser();

        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        Task<Dictionary<string, object>> GenerateJwtToken(User user);
        List<Claim> CreateUserClaims(User user);
        string FindUserIdFromClaims(ClaimsPrincipal user);
    }
}