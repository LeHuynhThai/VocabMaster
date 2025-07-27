using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

        [MaxLength(100)]
        public string Phonetic { get; set; }

        // Serialized JSON data for phonetics
        public string PhoneticsJson { get; set; } = "[]";

        // Serialized JSON data for meanings (including definitions, examples, etc.)
        [Required]
        public string MeaningsJson { get; set; }

        // Serialized JSON data for Vietnamese translations
        public string TranslationsJson { get; set; } = "{}";

        // When this record was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // When this record was last updated
        public DateTime? UpdatedAt { get; set; }
    }
} 