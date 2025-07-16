using VocabMaster.Entities;
using VocabMaster.Repositories.Interfaces;
using VocabMaster.Services.Interfaces;

namespace VocabMaster.Services.Implementations
{
    public class VocabularyService : IVocabularyService
    {
        private readonly IVocabularyRepository _repository;
        private readonly ILogger<VocabularyService> _logger;

        public VocabularyService(
            IVocabularyRepository repository,
            ILogger<VocabularyService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // get random word from database
        public async Task<Vocabulary> GetRandomVocabularyAsync()
        {
            try
            {
                var vocabulary = await _repository.GetRandomAsync(); // create random word by get random word from database
                if (vocabulary == null) // if no vocabulary, return null
                {
                    _logger.LogWarning("No vocabulary found");
                    return null;
                }
                // return random word
                return vocabulary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random vocabulary");
                throw;
            }
        }
    }
}


