using VocabMaster.Domain.Entities;

namespace VocabMaster.Domain.Interfaces
{
    public interface IAdminDashBoardRepo
    {
        Task<Vocabulary> AddVocabulary(Vocabulary vocabulary);
        Task<bool> DeleteVocabulary(int vocabularyId);
        Task<List<Vocabulary>> GetVocabularies();
    }
}
