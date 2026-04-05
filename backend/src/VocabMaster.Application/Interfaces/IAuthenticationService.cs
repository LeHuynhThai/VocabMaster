using VocabMaster.Application.Models.Authentication;
using VocabMaster.Domain.Entities;

namespace VocabMaster.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult?> Login(string name, string password);
        Task<bool> Register(string name, string password);
        Task Logout();
        Task<AuthenticationResult> GenerateJwtToken(User user);
    }
}