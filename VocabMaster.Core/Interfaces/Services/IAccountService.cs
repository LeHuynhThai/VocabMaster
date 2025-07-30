using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Interfaces.Services;

public interface IAccountService
{
    Task<TokenResponseDto> Login(string name, string password); // Cập nhật để trả về TokenResponseDto
    Task<bool> Register(User user); // Register
    string HashPassword(string password); // Hash password
    Task Logout(); // Logout
    Task<User> GetCurrentUser(); // Get current user
    Task<TokenResponseDto> GenerateJwtToken(User user); // Thêm phương thức tạo JWT token
    Task<TokenResponseDto> AuthenticateGoogleUser(GoogleAuthDto googleAuth); // Thêm phương thức xác thực Google
    Task<GoogleUserInfoDto> GetGoogleUserInfo(string accessToken); // Lấy thông tin người dùng Google
}
