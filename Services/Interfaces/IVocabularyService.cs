using VocabMaster.Entities;

namespace VocabMaster.Services.Interfaces;

public interface IVocabularyService
{
    Task<Vocabulary> GetRandomVocabularyAsync(); // Get random word
}

