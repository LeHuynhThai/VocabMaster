namespace VocabMaster.Core.DTOs
{
    /// <summary>
    /// DTO for LibreTranslate API request
    /// </summary>
    public class TranslationRequestDto
    {
        /// <summary>
        /// Text to translate
        /// </summary>
        public string Q { get; set; }
        
        /// <summary>
        /// Source language code (e.g., "en" for English)
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Target language code (e.g., "vi" for Vietnamese)
        /// </summary>
        public string Target { get; set; }
        
        /// <summary>
        /// API key (optional)
        /// </summary>
        public string ApiKey { get; set; }
    }

    /// <summary>
    /// DTO for LibreTranslate API response
    /// </summary>
    public class TranslationResponseDto
    {
        /// <summary>
        /// Translated text
        /// </summary>
        public string TranslatedText { get; set; }
        
        /// <summary>
        /// Original text that was translated
        /// </summary>
        public string OriginalText { get; set; }
        
        /// <summary>
        /// Source language code
        /// </summary>
        public string SourceLanguage { get; set; }
        
        /// <summary>
        /// Target language code
        /// </summary>
        public string TargetLanguage { get; set; }
    }
} 