using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Models.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(8, MinimumLength = 3, ErrorMessage = "Password must be 3 to 8 characters")]
        public string Password { get; set; }
    }
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(8, MinimumLength = 3, ErrorMessage = "Password must be 3 to 8 characters")]
        public string Password { get; set; }
    }
}