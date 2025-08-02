using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Translation;

namespace VocabMaster.Services.Translation
{
    public class VocabularyTranslationService : IVocabularyTranslationService
    {
        private readonly ITranslationService _translationService;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly ILogger<VocabularyTranslationService> _logger;
        private readonly int _delayBetweenRequestsMs;

        public VocabularyTranslationService(
            ITranslationService translationService,
            IVocabularyRepo vocabularyRepository,
            ILogger<VocabularyTranslationService> logger,
            IConfiguration configuration = null)
        {
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Get delay between requests from configuration if provided, otherwise use default
            _delayBetweenRequestsMs = configuration?.GetValue<int>("TranslationDelayMs") ?? 1000;
        }

        public async Task<int> CrawlAllTranslations()
        {
            try
            {
                _logger.LogInformation("Starting to crawl Vietnamese translations for all vocabulary");

                // Get all vocabularies without Vietnamese translation
                var vocabularies = await _vocabularyRepository.GetAll();
                if (vocabularies == null || !vocabularies.Any())
                {
                    _logger.LogWarning("No vocabularies found to translate");
                    return 0;
                }

                var untranslatedVocabularies = vocabularies.Where(v => string.IsNullOrEmpty(v.Vietnamese)).ToList();
                _logger.LogInformation("Found {Count} vocabularies without Vietnamese translation", untranslatedVocabularies.Count);

                int successCount = 0;
                int failCount = 0;

                // Process each vocabulary
                foreach (var vocabulary in untranslatedVocabularies)
                {
                    try
                    {
                        // Get translation from service
                        var translation = await _translationService.TranslateWord(vocabulary.Word);
                        if (string.IsNullOrEmpty(translation))
                        {
                            _logger.LogWarning("Could not get translation for word: {Word}, skipping", vocabulary.Word);
                            failCount++;
                            continue;
                        }

                        // Update vocabulary with translation
                        vocabulary.Vietnamese = translation;

                        // Save to database
                        await _vocabularyRepository.Update(vocabulary);

                        successCount++;
                        _logger.LogInformation("Successfully translated word: {Word} to {Translation}", vocabulary.Word, translation);

                        // Add a delay to avoid overloading the API
                        await Task.Delay(_delayBetweenRequestsMs);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing word: {Word}", vocabulary.Word);
                        failCount++;
                    }
                }

                _logger.LogInformation("Finished crawling translations. Success: {SuccessCount}, Failed: {FailCount}",
                    successCount, failCount);

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crawling all translations");
                return 0;
            }
        }
    }
} 