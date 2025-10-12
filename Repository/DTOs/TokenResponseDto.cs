namespace Repository.DTOs
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}