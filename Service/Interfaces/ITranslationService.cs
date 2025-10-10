namespace VocabMaster.Core.Interfaces.Services.Translation
{
    public interface ITranslationService
    {
        Task<string> TranslateWord(string word);
        Task<string> TranslateWordViaApi(string word);
        string TranslateWordFallback(string word);
        Task<int> CrawlAllTranslations();
    }
}