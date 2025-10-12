using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Repository.DTOs
{
    public class AddLearnedWordDto
    {
        [Required(ErrorMessage = "Từ vựng không được để trống")]
        [JsonPropertyName("word")]
        public string Word { get; set; }
    }
}