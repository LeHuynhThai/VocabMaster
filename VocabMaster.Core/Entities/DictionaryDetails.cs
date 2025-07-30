using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Core.Entities
{
    /// <summary>
    /// Entity to store detailed dictionary information for a word
    /// </summary>
    public class DictionaryDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Word { get; set; }

        // Serialized JSON data for phonetics
        public string PhoneticsJson { get; set; } = "[]";

        // Serialized JSON data for meanings (including definitions, examples, etc.)
        [Required]
        public string MeaningsJson { get; set; }

        // When this record was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // When this record was last updated
        public DateTime? UpdatedAt { get; set; }
    }
}