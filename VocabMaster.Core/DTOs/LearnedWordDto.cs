using System.Text.Json.Serialization;

namespace VocabMaster.Core.DTOs
{
    /// <summary>
    /// DTO for learned word information
    /// </summary>
    public class LearnedWordDto
    {
        /// <summary>
        /// ID of the learned word
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        /// <summary>
        /// The word text
        /// </summary>
        [JsonPropertyName("word")]
        public string Word { get; set; }
    }
} 