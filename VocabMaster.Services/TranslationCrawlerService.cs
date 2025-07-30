using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services
{
    /// <summary>
    /// Service for crawling Vietnamese translations for English vocabulary
    /// </summary>
    public class TranslationCrawlerService : ITranslationCrawlerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TranslationCrawlerService> _logger;
        private readonly IVocabularyRepo _vocabularyRepository;
        private readonly string _translationApiUrl;
        private readonly int _delayBetweenRequestsMs;

        /// <summary>
        /// Initializes a new instance of the TranslationCrawlerService
        /// </summary>
        /// <param name="logger">Logger for the service</param>
        /// <param name="vocabularyRepository">Repository for vocabulary operations</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="httpClientFactory">Factory for creating HttpClient instances</param>
        public TranslationCrawlerService(
            ILogger<TranslationCrawlerService> logger,
            IVocabularyRepo vocabularyRepository,
            IConfiguration configuration = null,
            IHttpClientFactory httpClientFactory = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));

            // Use IHttpClientFactory if provided, otherwise create a new HttpClient
            _httpClient = httpClientFactory != null ? httpClientFactory.CreateClient("TranslationApi") : new HttpClient();

            // Get API URL from configuration if provided, otherwise use default
            _translationApiUrl = configuration?.GetValue<string>("TranslationApiUrl") ?? "https://lingvanex.com/api/v1/translate";

            // Get delay between requests from configuration if provided, otherwise use default
            _delayBetweenRequestsMs = configuration?.GetValue<int>("TranslationDelayMs") ?? 1000;
        }

        /// <summary>
        /// Crawls Vietnamese translations for all English vocabulary in the database
        /// </summary>
        /// <returns>Number of translations successfully crawled</returns>
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
                        // Get translation from API
                        var translation = await TranslateWord(vocabulary.Word);
                        if (string.IsNullOrEmpty(translation))
                        {
                            _logger.LogWarning("Could not get translation for word: {Word}, skipping", vocabulary.Word);
                            failCount++;
                            continue;
                        }

                        // Update vocabulary with translation
                        vocabulary.Vietnamese = translation;

                        // Save to database
                        // Note: We're assuming the repository has an Update method
                        // If not, you'll need to implement a way to update the vocabulary
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

        /// <summary>
        /// Translates a single English word to Vietnamese
        /// </summary>
        /// <param name="word">The English word to translate</param>
        /// <returns>Vietnamese translation or null if not found</returns>
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

                // Try to use Google Translate API
                // Note: This is using a free endpoint that might not be stable long-term
                string googleTranslateUrl = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=vi&dt=t&q={Uri.EscapeDataString(word)}";

                var response = await _httpClient.GetAsync(googleTranslateUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Parse the response
                    // Google Translate API returns a nested array structure
                    // The first element of the first array contains the translation
                    try
                    {
                        // Using System.Text.Json to parse the response
                        using (JsonDocument document = JsonDocument.Parse(responseContent))
                        {
                            var root = document.RootElement;

                            // Navigate to the first translation
                            if (root.ValueKind == JsonValueKind.Array &&
                                root[0].ValueKind == JsonValueKind.Array &&
                                root[0][0].ValueKind == JsonValueKind.Array &&
                                root[0][0][0].ValueKind == JsonValueKind.String)
                            {
                                string translation = root[0][0][0].GetString();

                                if (!string.IsNullOrEmpty(translation))
                                {
                                    _logger.LogInformation("Successfully translated word: {Word} to {Translation}", word, translation);
                                    return translation;
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error parsing Google Translate response for word: {Word}", word);
                        // Fall through to fallback
                    }
                }

                // If Google Translate fails, try using another free translation API
                // You could implement additional API calls here

                // If all APIs fail, use the fallback dictionary
                _logger.LogWarning("API translation failed for word: {Word}, using fallback", word);
                return FallbackTranslate(word);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while translating word: {Word}", word);
                return FallbackTranslate(word);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error translating word: {Word}", word);
                return FallbackTranslate(word);
            }
        }

        /// <summary>
        /// Fallback translation method using a simple dictionary or another API
        /// </summary>
        /// <param name="word">The English word to translate</param>
        /// <returns>Vietnamese translation or null if not found</returns>
        private string FallbackTranslate(string word)
        {
            try
            {
                // This is a very simple fallback method
                // In a real application, you might want to use another API or a local dictionary
                var commonTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    // Common words
                    { "hello", "xin chào" },
                    { "world", "thế giới" },
                    { "computer", "máy tính" },
                    { "book", "sách" },
                    { "water", "nước" },
                    
                    // Body parts and actions
                    { "breathe", "thở" },
                    { "breath", "hơi thở" },
                    { "breathing", "hơi thở" },
                    { "head", "đầu" },
                    { "hand", "tay" },
                    { "foot", "chân" },
                    { "eye", "mắt" },
                    { "ear", "tai" },
                    { "mouth", "miệng" },
                    { "nose", "mũi" },
                    
                    // Common verbs
                    { "go", "đi" },
                    { "come", "đến" },
                    { "eat", "ăn" },
                    { "drink", "uống" },
                    { "sleep", "ngủ" },
                    { "walk", "đi bộ" },
                    { "run", "chạy" },
                    { "talk", "nói chuyện" },
                    { "speak", "nói" },
                    { "listen", "nghe" },
                    { "see", "nhìn" },
                    { "watch", "xem" },
                    { "read", "đọc" },
                    { "write", "viết" },
                    
                    // Common adjectives
                    { "good", "tốt" },
                    { "bad", "xấu" },
                    { "big", "lớn" },
                    { "small", "nhỏ" },
                    { "hot", "nóng" },
                    { "cold", "lạnh" },
                    { "new", "mới" },
                    { "old", "cũ" },
                    { "happy", "vui vẻ" },
                    { "sad", "buồn" },
                    
                    // Common nouns
                    { "man", "đàn ông" },
                    { "woman", "phụ nữ" },
                    { "child", "trẻ em" },
                    { "boy", "bé trai" },
                    { "girl", "bé gái" },
                    { "house", "nhà" },
                    { "car", "xe hơi" },
                    { "food", "thức ăn" },
                    { "time", "thời gian" },
                    { "day", "ngày" },
                    { "night", "đêm" },
                    { "year", "năm" },
                    { "month", "tháng" },
                    { "week", "tuần" },
                    
                    // Numbers
                    { "one", "một" },
                    { "two", "hai" },
                    { "three", "ba" },
                    { "four", "bốn" },
                    { "five", "năm" },
                    { "six", "sáu" },
                    { "seven", "bảy" },
                    { "eight", "tám" },
                    { "nine", "chín" },
                    { "ten", "mười" }
                };

                if (commonTranslations.TryGetValue(word, out string translation))
                {
                    _logger.LogInformation("Found fallback translation for word: {Word}", word);
                    return translation;
                }

                _logger.LogWarning("No fallback translation found for word: {Word}", word);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in fallback translation for word: {Word}", word);
                return null;
            }
        }

        /// <summary>
        /// Response class for translation API
        /// </summary>
        private class TranslationResponse
        {
            public string Result { get; set; }
            public string SourceLanguage { get; set; }
            public string TargetLanguage { get; set; }
        }
    }
}