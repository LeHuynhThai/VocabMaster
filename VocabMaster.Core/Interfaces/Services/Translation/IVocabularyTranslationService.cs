namespace VocabMaster.Core.Interfaces.Services.Translation
{
    public interface IVocabularyTranslationService
    {
        Task<int> CrawlAllTranslations();
    }
}