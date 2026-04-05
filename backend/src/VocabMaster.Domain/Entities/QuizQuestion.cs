using System.ComponentModel.DataAnnotations;
using VocabMaster.Domain.Common;

namespace VocabMaster.Domain.Entities
{
    public class QuizQuestion : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Word { get; set; } = string.Empty;

        [Required]
        public string CorrectAnswer { get; set; } = string.Empty;

        [Required]
        public string WrongAnswer1 { get; set; } = string.Empty;

        [Required]
        public string WrongAnswer2 { get; set; } = string.Empty;

        [Required]
        public string WrongAnswer3 { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}