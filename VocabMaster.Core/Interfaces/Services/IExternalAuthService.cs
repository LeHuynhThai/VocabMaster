using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services
{
    // authenticate and get user info from external auth providers
    public interface IExternalAuthService
    {
        Task<TokenResponseDto> AuthenticateGoogleUser(GoogleAuthDto googleAuth);
        
        Task<GoogleUserInfoDto> GetGoogleUserInfo(string accessToken);
    }
} 