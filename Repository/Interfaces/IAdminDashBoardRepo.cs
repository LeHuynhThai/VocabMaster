using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IAdminDashBoardRepo
    {
        Task<Vocabulary> AddVocabulary(Vocabulary vocabulary);
        Task<bool> DeleteVocabulary(int vocabularyId);
    }
}
