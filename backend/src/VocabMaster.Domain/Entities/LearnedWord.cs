using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VocabMaster.Domain.Common;

namespace VocabMaster.Domain.Entities
{
    public class LearnedWord : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Word { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime LearnedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}