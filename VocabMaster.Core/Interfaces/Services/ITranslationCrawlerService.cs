namespace VocabMaster.Core.Interfaces.Services
{
    public interface ITranslationCrawlerService
    {
        Task<int> CrawlAllTranslations();        
        Task<string> TranslateWord(string word);
    }
}