using System.Text.Json.Serialization;

namespace Repository.DTOs
{
    public class DictionaryResponseDto
    {
        [JsonPropertyName("word")]
        public string Word { get; set; }

        [JsonPropertyName("phonetic")]
        public string Phonetic { get; set; }

        [JsonPropertyName("phonetics")]
        public List<Phonetic> Phonetics { get; set; } = new List<Phonetic>();

        [JsonPropertyName("meanings")]
        public List<Meaning> Meanings { get; set; } = new List<Meaning>();

        [JsonPropertyName("vietnamese")]
        public string Vietnamese { get; set; }
        
        [JsonPropertyName("phoneticsJson")]
        public string PhoneticsJson { get; set; }
        
        [JsonPropertyName("meaningsJson")]
        public string MeaningsJson { get; set; }
    }

    public class Phonetic
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("audio")]
        public string Audio { get; set; }
    }

    public class Meaning
    {
        [JsonPropertyName("partOfSpeech")]
        public string PartOfSpeech { get; set; }

        [JsonPropertyName("definitions")]
        public List<Definition> Definitions { get; set; } = new List<Definition>();
    }

    public class Definition
    {
        [JsonPropertyName("definition")]
        public string Text { get; set; }

        [JsonPropertyName("example")]
        public string Example { get; set; }

        [JsonPropertyName("synonyms")]
        public List<string> Synonyms { get; set; } = new List<string>();

        [JsonPropertyName("antonyms")]
        public List<string> Antonyms { get; set; } = new List<string>();
    }
}