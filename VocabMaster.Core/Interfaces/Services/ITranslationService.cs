using System.Threading.Tasks;
using VocabMaster.Core.DTOs;

namespace VocabMaster.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for translation service operations
    /// </summary>
    public interface ITranslationService
    {
        /// <summary>
        /// Translates text from English to Vietnamese
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <returns>Translation response with translated text</returns>
        Task<TranslationResponseDto> TranslateEnglishToVietnamese(string text);
        
        /// <summary>
        /// Translates text between specified languages
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <param name="sourceLanguage">Source language code</param>
        /// <param name="targetLanguage">Target language code</param>
        /// <returns>Translation response with translated text</returns>
        Task<TranslationResponseDto> Translate(string text, string sourceLanguage, string targetLanguage);
    }
} 