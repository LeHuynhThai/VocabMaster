using System.ComponentModel.DataAnnotations;
using VocabMaster.Domain.Common;

namespace VocabMaster.Domain.Entities
{
    public class User : BaseEntity
    {
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
