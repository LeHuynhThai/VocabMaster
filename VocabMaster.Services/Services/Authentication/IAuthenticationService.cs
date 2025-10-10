using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<TokenResponseDto> Login(string name, string password);

        Task<bool> Register(User user);

        Task Logout();

        Task<User> GetCurrentUser();
    }
}