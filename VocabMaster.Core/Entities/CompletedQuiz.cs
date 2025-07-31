using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VocabMaster.Core.Entities
{
    public class CompletedQuiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int QuizQuestionId { get; set; }

        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public bool WasCorrect { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("QuizQuestionId")]
        public virtual QuizQuestion QuizQuestion { get; set; }
    }
}