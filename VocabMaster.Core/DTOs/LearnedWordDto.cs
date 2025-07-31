using System.Text.Json.Serialization;

namespace VocabMaster.Core.DTOs
{
    public class LearnedWordDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("word")]
        public string Word { get; set; }
    }
}