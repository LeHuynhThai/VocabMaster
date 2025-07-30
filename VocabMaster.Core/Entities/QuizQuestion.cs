using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Core.Entities
{
    /// <summary>
    /// Entity to store quiz questions for vocabulary testing
    /// </summary>
    public class QuizQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Word { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        [Required]
        public string WrongAnswer1 { get; set; }

        [Required]
        public string WrongAnswer2 { get; set; }

        [Required]
        public string WrongAnswer3 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 