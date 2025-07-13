using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Models
{
    public class Vocabulary
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Word { get; set; }

        [MaxLength(100)]
        public string Meaning { get; set; }

        [MaxLength(100)]
        public string Example { get; set; }

        [MaxLength(100)]
        public string? Pronunciation { get; set; }
    }
}
