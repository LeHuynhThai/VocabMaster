using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;

namespace VocabMaster.Core.DTOs
{
    /// <summary>
    /// Simplified vocabulary response DTO for API responses
    /// </summary>
    public class VocabularyResponseDto
    {
        /// <summary>
        /// Unique identifier for the vocabulary
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        /// <summary>
        /// The word text
        /// </summary>
        [JsonPropertyName("word")]
        public string Word { get; set; }
        
        /// <summary>
        /// Primary phonetic representation of the word
        /// </summary>
        [JsonPropertyName("phonetic")]
        public string Phonetic { get; set; }
        
        /// <summary>
        /// List of phonetic representations with audio
        /// </summary>
        [JsonPropertyName("pronunciations")]
        public List<PronunciationDto> Pronunciations { get; set; } = new List<PronunciationDto>();
        
        /// <summary>
        /// List of meanings with definitions and examples
        /// </summary>
        [JsonPropertyName("meanings")]
        public List<MeaningDto> Meanings { get; set; } = new List<MeaningDto>();
        
        /// <summary>
        /// Whether the word has been learned by the current user
        /// </summary>
        [JsonPropertyName("isLearned")]
        public bool IsLearned { get; set; }
        
        /// <summary>
        /// Creates a simplified response from a full dictionary response
        /// </summary>
        /// <param name="dictionaryResponse">The full dictionary response</param>
        /// <param name="id">Optional ID to assign</param>
        /// <param name="isLearned">Whether the word has been learned</param>
        /// <returns>A simplified vocabulary response</returns>
        public static VocabularyResponseDto FromDictionaryResponse(DictionaryResponseDto dictionaryResponse, int id = 0, bool isLearned = false)
        {
            if (dictionaryResponse == null)
                return null;
                
            var response = new VocabularyResponseDto
            {
                Id = id,
                Word = dictionaryResponse.Word,
                Phonetic = dictionaryResponse.Phonetic,
                IsLearned = isLearned
            };
            
            // Add pronunciations (up to 3)
            if (dictionaryResponse.Phonetics != null && dictionaryResponse.Phonetics.Count > 0)
            {
                // Prioritize phonetics with audio
                var phoneticsList = dictionaryResponse.Phonetics
                    .Where(p => !string.IsNullOrEmpty(p.Audio) || !string.IsNullOrEmpty(p.Text))
                    .Take(3)
                    .ToList();
                    
                foreach (var phonetic in phoneticsList)
                {
                    response.Pronunciations.Add(new PronunciationDto
                    {
                        Text = phonetic.Text,
                        Audio = phonetic.Audio
                    });
                }
            }
            
            // Add meanings (up to 3)
            if (dictionaryResponse.Meanings != null && dictionaryResponse.Meanings.Count > 0)
            {
                foreach (var meaning in dictionaryResponse.Meanings.Take(3))
                {
                    var meaningDto = new MeaningDto
                    {
                        PartOfSpeech = meaning.PartOfSpeech,
                        Definitions = new List<DefinitionDto>()
                    };
                    
                    // Add definitions (up to 3 per meaning)
                    if (meaning.Definitions != null && meaning.Definitions.Count > 0)
                    {
                        foreach (var definition in meaning.Definitions.Take(3))
                        {
                            meaningDto.Definitions.Add(new DefinitionDto
                            {
                                Text = definition.Text,
                                Example = definition.Example,
                                Synonyms = definition.Synonyms?.Take(5).ToList(),
                                Antonyms = definition.Antonyms?.Take(5).ToList()
                            });
                        }
                    }
                    
                    response.Meanings.Add(meaningDto);
                }
            }
            
            return response;
        }
    }
    
    /// <summary>
    /// Pronunciation information with phonetic text and audio
    /// </summary>
    public class PronunciationDto
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
    /// Meaning of a word with part of speech and definitions
    /// </summary>
    public class MeaningDto
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
        public List<DefinitionDto> Definitions { get; set; } = new List<DefinitionDto>();
    }
    
    /// <summary>
    /// Definition of a word with example and related words
    /// </summary>
    public class DefinitionDto
    {
        /// <summary>
        /// Definition text
        /// </summary>
        [JsonPropertyName("text")]
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