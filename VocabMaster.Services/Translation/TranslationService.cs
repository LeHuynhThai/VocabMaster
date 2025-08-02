using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VocabMaster.Core.Interfaces.Services.Translation;

namespace VocabMaster.Services.Translation
{
    public class TranslationService : ITranslationService
    {
        private readonly ITranslationApiService _translationApiService;
        private readonly IFallbackTranslationService _fallbackTranslationService;
        private readonly ILogger<TranslationService> _logger;

        public TranslationService(
            ITranslationApiService translationApiService,
            IFallbackTranslationService fallbackTranslationService,
            ILogger<TranslationService> logger)
        {
            _translationApiService = translationApiService ?? throw new ArgumentNullException(nameof(translationApiService));
            _fallbackTranslationService = fallbackTranslationService ?? throw new ArgumentNullException(nameof(fallbackTranslationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> TranslateWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Translating word: {Word}", word);

                // First try to translate using the API
                var apiTranslation = await _translationApiService.TranslateWordViaApi(word);
                if (!string.IsNullOrEmpty(apiTranslation))
                {
                    return apiTranslation;
                }

                // If API translation fails, use fallback
                _logger.LogWarning("API translation failed for word: {Word}, using fallback", word);
                return _fallbackTranslationService.TranslateWordFallback(word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error translating word: {Word}", word);
                
                // Try fallback as last resort
                try
                {
                    return _fallbackTranslationService.TranslateWordFallback(word);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
} 