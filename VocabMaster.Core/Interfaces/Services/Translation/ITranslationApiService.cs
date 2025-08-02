namespace VocabMaster.Core.Interfaces.Services.Translation
{
    // service that handle translate word via api
    public interface ITranslationApiService
    {
        Task<string> TranslateWordViaApi(string word);
    }
} 