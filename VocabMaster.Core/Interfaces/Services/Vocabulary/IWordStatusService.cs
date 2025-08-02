using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services.Vocabulary
{
    public interface IWordStatusService
    {
        Task<bool> IsWordLearned(int userId, string word);
        void InvalidateUserCache(int userId);
    }
} 