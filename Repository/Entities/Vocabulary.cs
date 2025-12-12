using System.ComponentModel.DataAnnotations;

namespace Repository.Entities
{
    public class Vocabulary
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Word { get; set; }
        [MaxLength(200)]
        public string Vietnamese { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
