using System.Threading.Tasks;

namespace VocabMaster.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for crawling Vietnamese translations for English vocabulary
    /// </summary>
    public interface ITranslationCrawlerService
    {
        /// <summary>
        /// Crawls Vietnamese translations for all English vocabulary in the database
        /// </summary>
        /// <returns>Number of translations successfully crawled</returns>
        Task<int> CrawlAllTranslations();
        
        /// <summary>
        /// Translates a single English word to Vietnamese
        /// </summary>
        /// <param name="word">The English word to translate</param>
        /// <returns>Vietnamese translation or null if not found</returns>
        Task<string> TranslateWord(string word);
    }
} 