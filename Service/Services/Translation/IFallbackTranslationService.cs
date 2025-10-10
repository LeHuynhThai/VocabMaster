namespace VocabMaster.Core.Interfaces.Services.Translation
{
    public interface IFallbackTranslationService
    {
        string TranslateWordFallback(string word);
    }
}