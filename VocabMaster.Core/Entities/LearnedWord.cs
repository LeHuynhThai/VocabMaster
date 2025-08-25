using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VocabMaster.Core.Entities
{
    // Entity đại diện cho một từ đã học của người dùng
    public class LearnedWord
    {
        [Key]
        public int Id { get; set; } // Khóa chính, định danh duy nhất cho bản ghi từ đã học

        [Required]
        [MaxLength(100)]
        public string Word { get; set; } // Từ vựng đã học

        [Required]
        public int UserId { get; set; } // Id của người dùng đã học từ này

        [Required]
        public DateTime LearnedAt { get; set; } = DateTime.UtcNow; // Thời điểm người dùng học từ này

        [ForeignKey("UserId")]
        public virtual User User { get; set; } // Thuộc tính điều hướng tới user đã học từ này
    }
}