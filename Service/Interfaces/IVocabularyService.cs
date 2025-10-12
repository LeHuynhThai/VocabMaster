using Repository.Entities;
namespace Service.Interfaces
{
    public interface IVocabularyService
    {
        Task<Vocabulary> GetRandomWord(int userId);
    }
}