using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VocabMaster.Domain.Common;

namespace VocabMaster.Domain.Entities
{
    public class CompletedQuiz : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int QuizQuestionId { get; set; }

        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public bool WasCorrect { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("QuizQuestionId")]
        public virtual QuizQuestion QuizQuestion { get; set; } = null!;
    }
}