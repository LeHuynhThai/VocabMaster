using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IVocabularyRepo
    {
        Task<Vocabulary> GetRandom();
        Task<Vocabulary> GetRandomExcludeLearned(List<string> learnedWords);
        Task<bool> Update(Vocabulary vocabulary);
        Task<bool> GetLearnedWords(int userId);
    }
}