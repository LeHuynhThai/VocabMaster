using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(3, ErrorMessage = "Password must be at least 3 characters")]
        public string Password { get; set; }

        public UserRole Role { get; set; } = UserRole.User; // Default role is User
    }
    public enum UserRole
    {
        User
    }
}
