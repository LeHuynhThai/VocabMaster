using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Api.Contracts.Account;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự")]
    public string Password { get; set; } = string.Empty;
}