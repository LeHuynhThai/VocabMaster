namespace VocabMaster.Core.Interfaces.Services.Translation
{
    public interface ITranslationService
    {
        Task<string> TranslateWord(string word);
    }
}