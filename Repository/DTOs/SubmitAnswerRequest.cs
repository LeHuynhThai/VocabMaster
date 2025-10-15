using System.ComponentModel.DataAnnotations;

namespace Repository.DTOs
{
    public class SubmitAnswerRequest
    {
        [Required]
        public int QuizQuestionId { get; set; }

        [Required]
        public string SelectedAnswer { get; set; } = string.Empty;
    }
}
