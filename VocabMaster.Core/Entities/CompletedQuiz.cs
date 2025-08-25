using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VocabMaster.Core.Entities
{
    // Entity đại diện cho một lần hoàn thành câu hỏi trắc nghiệm của user
    public class CompletedQuiz
    {
        [Key]
        public int Id { get; set; } // Khóa chính, định danh duy nhất cho bản ghi hoàn thành quiz

        [Required]
        public int UserId { get; set; } // Id của user đã hoàn thành quiz này

        [Required]
        public int QuizQuestionId { get; set; } // Id của câu hỏi trắc nghiệm đã hoàn thành

        [Required]
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow; // Thời điểm hoàn thành quiz

        public bool WasCorrect { get; set; } // Kết quả trả lời đúng hay sai

        // Thuộc tính điều hướng tới user đã hoàn thành quiz này
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Thuộc tính điều hướng tới câu hỏi trắc nghiệm đã hoàn thành
        [ForeignKey("QuizQuestionId")]
        public virtual QuizQuestion QuizQuestion { get; set; }
    }
}