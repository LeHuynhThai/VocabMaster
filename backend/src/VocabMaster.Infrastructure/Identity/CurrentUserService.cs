using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;

namespace VocabMaster.Infrastructure.Identity;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepo _userRepo;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserRepo userRepo)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
    }

    public int? GetUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("UserId")?.Value
            ?? principal.FindFirst("userId")?.Value
            ?? principal.FindFirst("id")?.Value
            ?? principal.FindFirst("sub")?.Value;

        return int.TryParse(userId, out var parsedUserId)
            ? parsedUserId
            : null;
    }

    public async Task<User?> GetCurrentUser()
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return null;
        }

        return await _userRepo.GetById(userId.Value);
    }
}