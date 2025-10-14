using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IVocabularyRepo
    {
        Task<Vocabulary> GetRandom();
        Task<Vocabulary> GetRandomExcludeLearned(List<string> learnedWords);
        Task<bool> Update(Vocabulary vocabulary);
        Task<List<LearnedWord>> GetLearnedWords(int userId);
        Task<LearnedWord> AddLearnedWord(LearnedWord learnedWord);
        Task<bool> RemoveLearnedWord(int learnedWordId);
        Task<Vocabulary?> GetVocabularyByWord(string word);
    }
}