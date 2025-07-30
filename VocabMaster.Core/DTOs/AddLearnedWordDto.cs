using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VocabMaster.Core.DTOs
{
    /// <summary>
    /// DTO for adding a word to learned list
    /// </summary>
    public class AddLearnedWordDto
    {
        /// <summary>
        /// Word to add to learned list
        /// </summary>
        [Required(ErrorMessage = "Từ vựng không được để trống")]
        [JsonPropertyName("word")]
        public string Word { get; set; }
    }
}