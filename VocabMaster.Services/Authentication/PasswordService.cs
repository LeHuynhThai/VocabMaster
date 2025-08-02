using Microsoft.Extensions.Logging;
using System;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services.Authentication
{
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(ILogger<PasswordService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
                
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

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