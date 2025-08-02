using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VocabMaster.Core.Interfaces.Services.Translation;

namespace VocabMaster.Services.Translation
{
    public class FallbackTranslationService : IFallbackTranslationService
    {
        private readonly ILogger<FallbackTranslationService> _logger;
        private readonly Dictionary<string, string> _commonTranslations;

        public FallbackTranslationService(ILogger<FallbackTranslationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commonTranslations = InitializeCommonTranslations();
        }

        public string TranslateWordFallback(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return null;
            }

            try
            {
                if (_commonTranslations.TryGetValue(word.ToLowerInvariant(), out string translation))
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

        private Dictionary<string, string> InitializeCommonTranslations()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
        }
    }
} 