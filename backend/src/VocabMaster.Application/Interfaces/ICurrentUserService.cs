using VocabMaster.Domain.Entities;

namespace VocabMaster.Application.Interfaces;

public interface ICurrentUserService
{
    int? GetUserId();

    Task<User?> GetCurrentUser();
}