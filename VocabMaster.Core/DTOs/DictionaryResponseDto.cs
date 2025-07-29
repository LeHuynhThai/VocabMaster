using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VocabMaster.Core.DTOs
{
    /// <summary>
    /// Response from the dictionary API
    /// </summary>
    public class DictionaryResponseDto
    {
        /// <summary>
        /// The word
        /// </summary>
        [JsonPropertyName("word")]
        public string Word { get; set; }
        
        /// <summary>
        /// Phonetic representation of the word
        /// </summary>
        [JsonPropertyName("phonetic")]
        public string Phonetic { get; set; }
        
        /// <summary>
        /// List of phonetics with audio
        /// </summary>
        [JsonPropertyName("phonetics")]
        public List<Phonetic> Phonetics { get; set; } = new List<Phonetic>();
        
        /// <summary>
        /// List of meanings with definitions
        /// </summary>
        [JsonPropertyName("meanings")]
        public List<Meaning> Meanings { get; set; } = new List<Meaning>();

        /// <summary>
        /// Vietnamese translation of the word
        /// </summary>
        [JsonPropertyName("vietnamese")]
        public string Vietnamese { get; set; }
    }
    
    /// <summary>
    /// Phonetic information
    /// </summary>
    public class Phonetic
    {
        /// <summary>
        /// Phonetic text representation
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }
        
        /// <summary>
        /// Audio URL for pronunciation
        /// </summary>
        [JsonPropertyName("audio")]
        public string Audio { get; set; }
    }
    
    /// <summary>
    /// Meaning of a word
    /// </summary>
    public class Meaning
    {
        /// <summary>
        /// Part of speech (noun, verb, adjective, etc.)
        /// </summary>
        [JsonPropertyName("partOfSpeech")]
        public string PartOfSpeech { get; set; }
        
        /// <summary>
        /// List of definitions
        /// </summary>
        [JsonPropertyName("definitions")]
        public List<Definition> Definitions { get; set; } = new List<Definition>();
    }
    
    /// <summary>
    /// Definition of a word
    /// </summary>
    public class Definition
    {
        /// <summary>
        /// Definition text
        /// </summary>
        [JsonPropertyName("definition")]
        public string Text { get; set; }
        
        /// <summary>
        /// Example usage of the word
        /// </summary>
        [JsonPropertyName("example")]
        public string Example { get; set; }
        
        /// <summary>
        /// List of synonyms
        /// </summary>
        [JsonPropertyName("synonyms")]
        public List<string> Synonyms { get; set; } = new List<string>();
        
        /// <summary>
        /// List of antonyms
        /// </summary>
        [JsonPropertyName("antonyms")]
        public List<string> Antonyms { get; set; } = new List<string>();
    }
} 