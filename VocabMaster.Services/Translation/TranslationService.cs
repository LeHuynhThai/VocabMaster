using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Services.Translation
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly ILogger<TranslationService> _logger;
        private readonly int _delayBetweenRequestsMs;

        public TranslationService(
            IVocabularyRepo vocabularyRepository,
            ILogger<TranslationService> logger,
            IConfiguration configuration = null,
            HttpClient httpClient = null)
        {
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _delayBetweenRequestsMs = configuration?.GetValue<int>("TranslationDelayMs") ?? 1000;
            _httpClient = httpClient;
        }

        public async Task<int> CrawlAllTranslations()
        {
            try
            {
                _logger.LogInformation("Starting to crawl Vietnamese translations for all vocabulary");

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

                foreach (var vocabulary in untranslatedVocabularies)
                {
                    try
                    {
                        var translation = await TranslateWordViaApi(vocabulary.Word);
                        if (string.IsNullOrEmpty(translation))
                        {
                            _logger.LogWarning("Could not get translation for word: {Word}, skipping", vocabulary.Word);
                            failCount++;
                            continue;
                        }

                        vocabulary.Vietnamese = translation;
                        await _vocabularyRepository.Update(vocabulary);

                        successCount++;
                        _logger.LogInformation("Successfully translated word: {Word} to {Translation}", vocabulary.Word, translation);

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

        private async Task<string> TranslateWordViaApi(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Translating word via API: {Word}", word);

                string apiUrl = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=vi&dt=t&q={Uri.EscapeDataString(word)}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    try
                    {
                        using (JsonDocument document = JsonDocument.Parse(content))
                        {
                            var root = document.RootElement;
                            
                            if (root.ValueKind == JsonValueKind.Array &&
                                root[0].ValueKind == JsonValueKind.Array &&
                                root[0][0].ValueKind == JsonValueKind.Array &&
                                root[0][0][0].ValueKind == JsonValueKind.String)
                            {
                                string translation = root[0][0][0].GetString();

                                if (!string.IsNullOrEmpty(translation))
                                {
                                    _logger.LogInformation("Successfully translated word via API: {Word} to {Translation}", word, translation);
                                    return translation;
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error parsing Google Translate response for word: {Word}", word);
                    }
                }

                _logger.LogWarning("API translation failed for word: {Word}", word);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while translating word: {Word}", word);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error translating word via API: {Word}", word);
                return null;
            }
        }
    }
}
