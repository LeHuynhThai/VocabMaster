using Microsoft.EntityFrameworkCore;
using VocabMaster.Models;

namespace VocabMaster.Data.SeedData
{
    public static class VocabularySeeder
    {
        public static async Task SeedVocabularies(AppDbContext context)
        {
            if (await context.Vocabularies.AnyAsync())
                return; // Đã có dữ liệu rồi thì không seed nữa

            var vocabularies = new List<Vocabulary>
            {
                new Vocabulary 
                {
                    Word = "abandon",
                    Meaning = "từ bỏ, bỏ rơi",
                    Example = "He abandoned his car and walked home.",
                    Pronunciation = "əˈbændən"
                },
                new Vocabulary 
                {
                    Word = "ability",
                    Meaning = "khả năng, năng lực",
                    Example = "She has the ability to speak five languages.",
                    Pronunciation = "əˈbɪləti"
                },
                new Vocabulary 
                {
                    Word = "abroad",
                    Meaning = "ở nước ngoài",
                    Example = "He studied abroad for two years.",
                    Pronunciation = "əˈbrɔːd"
                },
                new Vocabulary 
                {
                    Word = "accept",
                    Meaning = "chấp nhận",
                    Example = "I accept your apology.",
                    Pronunciation = "əkˈsept"
                },
                // Thêm ~96 từ nữa ở đây...
                new Vocabulary 
                {
                    Word = "achieve",
                    Meaning = "đạt được",
                    Example = "She achieved her goal of becoming a doctor.",
                    Pronunciation = "əˈtʃiːv"
                },
                new Vocabulary 
                {
                    Word = "adventure",
                    Meaning = "cuộc phiêu lưu",
                    Example = "We had an exciting adventure in the mountains.",
                    Pronunciation = "ədˈventʃər"
                },
                new Vocabulary 
                {
                    Word = "afraid",
                    Meaning = "sợ hãi",
                    Example = "Don't be afraid of the dark.",
                    Pronunciation = "əˈfreɪd"
                },
                // ... thêm các từ khác
            };

            await context.Vocabularies.AddRangeAsync(vocabularies);
            await context.SaveChangesAsync();
        }
    }
}
