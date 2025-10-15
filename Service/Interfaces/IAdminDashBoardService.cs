using Repository.Entities;

namespace Service.Interfaces
{
    public interface IAdminDashBoardService
    {
        Task<Vocabulary> AddVocabulary(Vocabulary vocabulary);
        Task<bool> DeleteVocabulary(int vocabularyId);
    }
}
