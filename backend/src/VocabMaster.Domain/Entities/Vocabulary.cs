using System.ComponentModel.DataAnnotations;
using VocabMaster.Domain.Common;

namespace VocabMaster.Domain.Entities
{
    public class Vocabulary : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Word { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Vietnamese { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
