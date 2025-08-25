using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Core.Entities
{
    // Entity đại diện cho một câu hỏi trắc nghiệm trong hệ thống
    public class QuizQuestion
    {
        [Key]
        public int Id { get; set; } // Khóa chính, định danh duy nhất cho mỗi câu hỏi

        [Required]
        [MaxLength(100)]
        public string Word { get; set; } // Từ vựng dùng làm câu hỏi

        [Required]
        public string CorrectAnswer { get; set; } // Đáp án đúng cho từ vựng này

        [Required]
        public string WrongAnswer1 { get; set; } // Đáp án sai 1

        [Required]
        public string WrongAnswer2 { get; set; } // Đáp án sai 2

        [Required]
        public string WrongAnswer3 { get; set; } // Đáp án sai 3

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Thời điểm tạo câu hỏi
    }
}