using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Dictionary;

namespace VocabMaster.Services.Dictionary
{
    // Service cache dữ liệu định nghĩa từ vựng vào database từ API
    public class DictionaryCacheService : IDictionaryCacheService
    {
        private readonly ILogger<DictionaryCacheService> _logger;
        private readonly IVocabularyRepo _vocabularyRepository;
        private const string API_URL = "https://api.dictionaryapi.dev/api/v2/entries/en/";

        // Hàm khởi tạo service, inject logger và repository
        public DictionaryCacheService(
            ILogger<DictionaryCacheService> logger,
            IVocabularyRepo vocabularyRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
        }

        // Cache toàn bộ định nghĩa từ vựng từ API vào database
        public async Task<int> CacheAllVocabularyDefinitions()
        {
            try
            {
                _logger.LogInformation("Start caching all vocabulary");

                // Lấy toàn bộ từ vựng từ database
                var vocabularies = await _vocabularyRepository.GetAll();
                if (vocabularies == null || !vocabularies.Any())
                {
                    _logger.LogWarning("No vocabulary to cache");
                    return 0;
                }

                _logger.LogInformation("Found {Count} vocabulary", vocabularies.Count);

                int successCount = 0;
                int failCount = 0;

                // Tạo mới HttpClient cho mỗi lần gọi hàm
                using (var httpClient = new HttpClient())
                {
                    // Duyệt từng từ vựng
                    foreach (var vocab in vocabularies)
                    {
                        try
                        {
                            // Kiểm tra từ đã có dữ liệu đầy đủ chưa
                            bool needsUpdate = string.IsNullOrEmpty(vocab.MeaningsJson) || vocab.MeaningsJson == "[]";
                            
                            if (needsUpdate)
                            {
                                // Gọi API dictionary
                                string apiUrl = $"{API_URL}{Uri.EscapeDataString(vocab.Word)}";
                                var response = await httpClient.GetAsync(apiUrl);

                                if (response.IsSuccessStatusCode)
                                {
                                    var content = await response.Content.ReadAsStringAsync();
                                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                    var dictionaryResponse = JsonSerializer.Deserialize<List<DictionaryResponseDto>>(content, options);
                                    var definition = dictionaryResponse?.FirstOrDefault();

                                    if (definition != null)
                                    {
                                        // Cập nhật dữ liệu từ API
                                        vocab.PhoneticsJson = definition.Phonetics != null && definition.Phonetics.Any()
                                            ? JsonSerializer.Serialize(definition.Phonetics, options)
                                            : "[]";
                                            
                                        vocab.MeaningsJson = definition.Meanings != null
                                            ? JsonSerializer.Serialize(definition.Meanings, options)
                                            : "[]";
                                            
                                        _logger.LogInformation("Successfully got definition from API for word: {Word}", vocab.Word);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("No definition found for word: {Word}", vocab.Word);
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("API returned error {StatusCode} for word {Word}", response.StatusCode, vocab.Word);
                                }
                            }
                            
                            // Luôn cập nhật metadata
                            vocab.UpdatedAt = DateTime.UtcNow;
                            
                            // Lưu vào database
                            var success = await _vocabularyRepository.Update(vocab);
                            
                            if (success)
                            {
                                successCount++;
                                _logger.LogInformation("Successfully cached word: {Word}", vocab.Word);
                            }
                            else
                            {
                                failCount++;
                                _logger.LogWarning("Failed to cache word: {Word}", vocab.Word);
                            }

                            // Đợi để tránh spam API
                            if (needsUpdate)
                            {
                                await Task.Delay(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing word: {Word}", vocab.Word);
                            failCount++;
                        }
                    }
                }

                _logger.LogInformation("Finished caching vocabulary. Success: {Success}, Failed: {Fail}", 
                    successCount, failCount);

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching vocabulary");
                return 0;
            }
        }
    }
}