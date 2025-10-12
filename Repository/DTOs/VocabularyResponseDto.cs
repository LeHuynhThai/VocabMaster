using System.Text.Json.Serialization;

namespace Repository.DTOs
{
    public class VocabularyResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("word")]
        public string Word { get; set; }

        [JsonPropertyName("phonetic")]
        public string Phonetic { get; set; }

        [JsonPropertyName("pronunciations")]
        public List<PronunciationDto> Pronunciations { get; set; } = new List<PronunciationDto>();

        [JsonPropertyName("meanings")]
        public List<MeaningDto> Meanings { get; set; } = new List<MeaningDto>();

        [JsonPropertyName("isLearned")]
        public bool IsLearned { get; set; }

        [JsonPropertyName("vietnamese")]
        public string Vietnamese { get; set; }

        [JsonPropertyName("phoneticsJson")]
        public string PhoneticsJson { get; set; }

        [JsonPropertyName("meaningsJson")]
        public string MeaningsJson { get; set; }

        public static VocabularyResponseDto FromDictionaryResponse(DictionaryResponseDto dictionaryResponse, int id = 0, bool isLearned = false, string vietnamese = null)
        {
            if (dictionaryResponse == null)
                return null;

            var response = new VocabularyResponseDto
            {
                Id = id,
                Word = dictionaryResponse.Word,
                Phonetic = dictionaryResponse.Phonetic,
                IsLearned = isLearned,
                Vietnamese = vietnamese,
                PhoneticsJson = dictionaryResponse.PhoneticsJson,
                MeaningsJson = dictionaryResponse.MeaningsJson
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

    public class PronunciationDto
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("audio")]
        public string Audio { get; set; }
    }

    public class MeaningDto
    {
        [JsonPropertyName("partOfSpeech")]
        public string PartOfSpeech { get; set; }

        [JsonPropertyName("definitions")]
        public List<DefinitionDto> Definitions { get; set; } = new List<DefinitionDto>();
    }

    public class DefinitionDto
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("example")]
        public string Example { get; set; }

        [JsonPropertyName("synonyms")]
        public List<string> Synonyms { get; set; } = new List<string>();

        [JsonPropertyName("antonyms")]
        public List<string> Antonyms { get; set; } = new List<string>();
    }
}