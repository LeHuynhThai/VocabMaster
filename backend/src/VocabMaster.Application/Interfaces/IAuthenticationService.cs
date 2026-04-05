using System.Security.Claims;
using VocabMaster.Domain.Entities;

namespace VocabMaster.Application.Interfaces
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