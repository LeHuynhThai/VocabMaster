using System.Text.Json.Serialization;

namespace VocabMaster.Core.DTOs
{
    public class GoogleAuthDto
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("idToken")]
        public string IdToken { get; set; }
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