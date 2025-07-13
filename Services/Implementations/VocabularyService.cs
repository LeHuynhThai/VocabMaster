using VocabMaster.Entities;
using VocabMaster.Repositories;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Services.Implementations;

public class VocabularyService : IVocabularyService
{
    private readonly IVocabularyRepository _repository; // Vocabulary repository

    public VocabularyService(IVocabularyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Vocabulary> GetRandomVocabularyAsync()
    {
        return await _repository.GetRandomAsync();
    }
}


