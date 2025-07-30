using System.Text.Json.Serialization;

namespace VocabMaster.Core.DTOs
{
    public class GoogleAuthDto
    {
        [JsonPropertyName("idToken")]
        public string IdToken { get; set; }
        
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
    }

    public class GoogleUserInfoDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("picture")]
        public string Picture { get; set; }
    }
} 