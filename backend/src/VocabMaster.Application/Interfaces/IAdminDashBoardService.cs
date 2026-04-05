using VocabMaster.Domain.Entities;

namespace VocabMaster.Application.Interfaces
{
    public interface IAdminDashBoardService
    {
        Task<Vocabulary> AddVocabulary(Vocabulary vocabulary);
        Task<bool> DeleteVocabulary(int vocabularyId);
        Task<List<Vocabulary>> GetVocabularies();
        Task<Vocabulary> CrawFromApi(string englishWord);
    }
}
