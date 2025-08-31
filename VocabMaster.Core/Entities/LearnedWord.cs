using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VocabMaster.Core.Entities
{
    public class LearnedWord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Word { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime LearnedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}