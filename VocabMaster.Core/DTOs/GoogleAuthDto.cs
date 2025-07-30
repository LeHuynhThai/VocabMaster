using System.Text.Json.Serialization;

namespace VocabMaster.Core.DTOs
{
    /// <summary>
    /// DTO đại diện cho token xác thực từ Google
    /// </summary>
    public class GoogleAuthDto
    {
        /// <summary>
        /// Token xác thực từ Google OAuth
        /// </summary>
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
        
        /// <summary>
        /// ID token nếu có (tùy chọn)
        /// </summary>
        [JsonPropertyName("idToken")]
        public string IdToken { get; set; }
    }

    /// <summary>
    /// DTO đại diện cho thông tin người dùng Google
    /// </summary>
    public class GoogleUserInfoDto
    {
        /// <summary>
        /// ID người dùng Google
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Email của người dùng Google
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        /// <summary>
        /// Tên đầy đủ của người dùng Google
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// URL ảnh đại diện của người dùng Google
        /// </summary>
        [JsonPropertyName("picture")]
        public string Picture { get; set; }
        
        /// <summary>
        /// Email đã xác minh hay chưa
        /// </summary>
        [JsonPropertyName("email_verified")]
        public bool? EmailVerified { get; set; }

        [JsonPropertyName("sub")]
        public string Subject { get; set; }

        public override string ToString()
        {
            return $"GoogleUserInfo[Id={Id}, Email={Email}, Name={Name}]";
        }
    }
} 