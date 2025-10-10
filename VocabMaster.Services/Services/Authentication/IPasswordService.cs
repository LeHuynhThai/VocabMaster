namespace VocabMaster.Core.Interfaces.Services
{
    // hash and verify password
    public interface IPasswordService
    {
        string HashPassword(string password);

        bool VerifyPassword(string password, string hash);
    }
}