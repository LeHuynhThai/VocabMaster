using System.ComponentModel.DataAnnotations;

namespace Repository.DTOs
{
    public class AddVocabularyRequestDto
    {
        [Required]
        public string Word { get; set; } = string.Empty;

        [Required]
        public string Vietnamese { get; set; } = string.Empty;

        public string MeaningsJson { get; set; } = "[]";

        public string PronunciationsJson { get; set; } = "[]";
    }

    public class VocabularyResponseDto
    {
        public int Id { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Vietnamese { get; set; } = string.Empty;
        public string MeaningsJson { get; set; } = string.Empty;
        public string PronunciationsJson { get; set; } = string.Empty;
    }
}
