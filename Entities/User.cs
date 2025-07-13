using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Tên không quá 50 ký tự")]
        public string Name { get; set; }
    }
}
