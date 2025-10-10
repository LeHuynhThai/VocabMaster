using System.Security.Claims;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services
{
    // create and manage JWT token
    public interface ITokenService
    {
        Task<TokenResponseDto> GenerateJwtToken(User user);

        List<Claim> CreateUserClaims(User user);

        string FindUserIdFromClaims(ClaimsPrincipal user);
    }
}