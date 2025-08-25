using Microsoft.Extensions.Logging;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services.Authentication
{
    // Service xử lý logic liên quan đến mật khẩu (băm, kiểm tra...)
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;

        // Hàm khởi tạo service, inject logger
        public PasswordService(ILogger<PasswordService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Băm mật khẩu thành chuỗi hash
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Kiểm tra mật khẩu với hash
        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash cannot be null or empty", nameof(hash));

            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}