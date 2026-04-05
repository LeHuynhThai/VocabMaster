using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Api.Contracts.Vocabulary;

public sealed class AddLearnedWordRequest
{
    [Required(ErrorMessage = "Từ vựng là bắt buộc")]
    [StringLength(100, ErrorMessage = "Từ vựng không được vượt quá 100 ký tự")]
    public string Word { get; set; } = string.Empty;
}