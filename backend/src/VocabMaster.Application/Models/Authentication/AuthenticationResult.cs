namespace VocabMaster.Application.Models.Authentication
{
    public sealed class AuthenticationResult
    {
        public string AccessToken { get; init; } = string.Empty;

        public int ExpiresIn { get; init; }

        public int UserId { get; init; }

        public string UserName { get; init; } = string.Empty;
    }
}