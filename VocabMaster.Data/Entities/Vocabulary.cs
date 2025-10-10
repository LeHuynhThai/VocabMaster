using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Core.Entities
{
    public class Vocabulary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Word { get; set; }

        [MaxLength(200)]
        public string Vietnamese { get; set; }

        public string PhoneticsJson { get; set; } = "[]";

        public string MeaningsJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
