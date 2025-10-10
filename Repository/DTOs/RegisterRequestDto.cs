using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Core.DTOs
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(8, MinimumLength = 3, ErrorMessage = "Password must be 3 to 8 characters")]
        public string Password { get; set; }
    }
}
