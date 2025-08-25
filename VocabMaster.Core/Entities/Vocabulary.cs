using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Core.Entities
{
    // Entity đại diện cho một từ vựng trong hệ thống
    public class Vocabulary
    {
        [Key]
        public int Id { get; set; } // Khóa chính, định danh duy nhất cho từ vựng

        [Required]
        [MaxLength(100)]
        public string Word { get; set; } // Từ vựng tiếng Anh

        [MaxLength(200)]
        public string Vietnamese { get; set; } // Nghĩa tiếng Việt của từ

        public string PhoneticsJson { get; set; } = "[]"; // Dữ liệu phiên âm (dạng JSON)

        public string MeaningsJson { get; set; } // Dữ liệu nghĩa chi tiết (dạng JSON)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Thời điểm tạo từ vựng

        public DateTime? UpdatedAt { get; set; } // Thời điểm cập nhật gần nhất (nếu có)
    }
}
