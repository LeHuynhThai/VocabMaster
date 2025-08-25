using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Core.Entities
{
    // Entity đại diện cho người dùng trong hệ thống
    public class User
    {
        [Key]
        public int Id { get; set; } // Khóa chính, định danh duy nhất cho user

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // Tên người dùng

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } // Mật khẩu đã mã hóa

        public UserRole Role { get; set; } = UserRole.User; // Vai trò của user (User/Admin...)

        // Danh sách các từ đã học của user (không trùng lặp)
        public virtual ICollection<LearnedWord> LearnedVocabularies { get; set; } = new HashSet<LearnedWord>();
    }

    // Enum định nghĩa các vai trò của user
    public enum UserRole
    {
        User // Người dùng thông thường
    }
}
