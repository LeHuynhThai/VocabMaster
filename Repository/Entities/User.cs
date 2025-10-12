using System.ComponentModel.DataAnnotations;

namespace Repository.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public virtual ICollection<LearnedWord> LearnedVocabularies { get; set; } = new HashSet<LearnedWord>();
    }

    public enum UserRole
    {
        User,
        Admin
    }
}
