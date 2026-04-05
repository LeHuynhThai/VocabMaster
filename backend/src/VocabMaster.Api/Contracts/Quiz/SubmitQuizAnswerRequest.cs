using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Api.Contracts.Quiz;

public sealed class SubmitQuizAnswerRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã câu hỏi không hợp lệ")]
    public int QuizQuestionId { get; set; }

    [Required(ErrorMessage = "Đáp án là bắt buộc")]
    public string SelectedAnswer { get; set; } = string.Empty;
}