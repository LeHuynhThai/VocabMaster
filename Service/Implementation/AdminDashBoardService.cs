using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;
using System.Text.Json;
using System.Net.Http;

namespace Service.Implementation
{
    public class AdminDashBoardService : IAdminDashBoardService
    {
        private readonly IAdminDashBoardRepo _adminDashBoardRepo;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminDashBoardService(IAdminDashBoardRepo adminDashBoardRepo, IHttpClientFactory httpClientFactory)
        {
            _adminDashBoardRepo = adminDashBoardRepo;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Vocabulary> AddVocabulary(Vocabulary vocabulary)
        {
            return await _adminDashBoardRepo.AddVocabulary(vocabulary);
        }

        public async Task<bool> DeleteVocabulary(int vocabularyId)
        {
            return await _adminDashBoardRepo.DeleteVocabulary(vocabularyId);
        }

        public async Task<List<Vocabulary>> GetVocabularies()
        {
            return await _adminDashBoardRepo.GetVocabularies();
        }

        public async Task<Vocabulary> CrawFromApi(string englishWord)
        {
            if (string.IsNullOrWhiteSpace(englishWord))
                throw new ArgumentException("Word is required", nameof(englishWord));

            var word = englishWord.Trim();

            // 1) DictionaryAPI
            var dictClient = _httpClientFactory.CreateClient("DictionaryApi");
            using var dictRes = await dictClient.GetAsync($"api/v2/entries/en/{Uri.EscapeDataString(word)}");
            dictRes.EnsureSuccessStatusCode();
            var dictJson = await dictRes.Content.ReadAsStringAsync();

            string phoneticsJson = "[]";
            string meaningsJson = "[]";

            try
            {
                using var doc = JsonDocument.Parse(dictJson);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var first = root[0];
                    if (first.TryGetProperty("phonetics", out var ph))
                        phoneticsJson = ph.GetRawText();
                    if (first.TryGetProperty("meanings", out var me))
                        meaningsJson = me.GetRawText();
                }
            }
            catch
            {
                // keep defaults
            }

            // 2) Google Translate (to Vietnamese)
            var transClient = _httpClientFactory.CreateClient("GoogleTranslate");
            var url = $"translate_a/t?client=any_client_id_works&sl=auto&tl=vi&q={Uri.EscapeDataString(word)}&tbb=1&ie=UTF-8&oe=UTF-8";
            using var transRes = await transClient.GetAsync(url);
            transRes.EnsureSuccessStatusCode();
            var transRaw = await transRes.Content.ReadAsStringAsync();

            string vietnamese = word;
            try
            {
                using var tdoc = JsonDocument.Parse(transRaw);
                if (tdoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var r = tdoc.RootElement[0];
                    if (r.ValueKind == JsonValueKind.String)
                        vietnamese = r.GetString() ?? word;
                    else if (r.ValueKind == JsonValueKind.Array && r.GetArrayLength() > 0 && r[0].ValueKind == JsonValueKind.String)
                        vietnamese = r[0].GetString() ?? word;
                }
            }
            catch
            {
                // keep default
            }

            var vocabulary = new Vocabulary
            {
                Word = word,
                Vietnamese = vietnamese,
                PhoneticsJson = phoneticsJson,
                MeaningsJson = meaningsJson
            };

            return await _adminDashBoardRepo.AddVocabulary(vocabulary);
        }
    }
}
