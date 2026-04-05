using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VocabMaster.Domain.Entities;
using VocabMaster.Infrastructure.Persistence;

namespace VocabMaster.Infrastructure.Persistence.SeedData
{
    public class SeedVocabulary
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                await context.Database.EnsureCreatedAsync();

                await SeedVocabularyData(context);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task SeedVocabularyData(ApplicationDbContext context)
        {
            // Load existing words and insert only missing ones (idempotent)
            var existingWordsList = await context.Vocabularies
                .Select(v => v.Word)
                .ToListAsync();
            var existingWords = new HashSet<string>(existingWordsList, StringComparer.OrdinalIgnoreCase);

            var vocabularies = new List<Vocabulary>
            {
                new Vocabulary { Word = "apple", Vietnamese = "táo" },
                new Vocabulary { Word = "animal", Vietnamese = "động vật" },
                new Vocabulary { Word = "answer", Vietnamese = "câu trả lời" },
                new Vocabulary { Word = "airport", Vietnamese = "sân bay" },
                new Vocabulary { Word = "book", Vietnamese = "sách" },
                new Vocabulary { Word = "bridge", Vietnamese = "cầu" },
                new Vocabulary { Word = "brother", Vietnamese = "anh/em trai" },
                new Vocabulary { Word = "beach", Vietnamese = "bãi biển" },
                new Vocabulary { Word = "car", Vietnamese = "xe hơi" },
                new Vocabulary { Word = "city", Vietnamese = "thành phố" },
                new Vocabulary { Word = "chair", Vietnamese = "ghế" },
                new Vocabulary { Word = "cold", Vietnamese = "lạnh" },
                new Vocabulary { Word = "dog", Vietnamese = "con chó" },
                new Vocabulary { Word = "door", Vietnamese = "cửa" },
                new Vocabulary { Word = "doctor", Vietnamese = "bác sĩ" },
                new Vocabulary { Word = "dream", Vietnamese = "giấc mơ" },
                new Vocabulary { Word = "earth", Vietnamese = "trái đất" },
                new Vocabulary { Word = "eat", Vietnamese = "ăn" },
                new Vocabulary { Word = "expensive", Vietnamese = "đắt" },
                new Vocabulary { Word = "energy", Vietnamese = "năng lượng" },
                new Vocabulary { Word = "family", Vietnamese = "gia đình" },
                new Vocabulary { Word = "friend", Vietnamese = "bạn bè" },
                new Vocabulary { Word = "flower", Vietnamese = "bông hoa" },
                new Vocabulary { Word = "food", Vietnamese = "thức ăn" },
                new Vocabulary { Word = "garden", Vietnamese = "vườn" },
                new Vocabulary { Word = "game", Vietnamese = "trò chơi" },
                new Vocabulary { Word = "green", Vietnamese = "xanh lá" },
                new Vocabulary { Word = "good", Vietnamese = "tốt" },
                new Vocabulary { Word = "house", Vietnamese = "ngôi nhà" },
                new Vocabulary { Word = "happy", Vietnamese = "vui vẻ" },
                new Vocabulary { Word = "history", Vietnamese = "lịch sử" },
                new Vocabulary { Word = "human", Vietnamese = "con người" },
                new Vocabulary { Word = "island", Vietnamese = "hòn đảo" },
                new Vocabulary { Word = "idea", Vietnamese = "ý tưởng" },
                new Vocabulary { Word = "image", Vietnamese = "hình ảnh" },
                new Vocabulary { Word = "improve", Vietnamese = "cải thiện" },
                new Vocabulary { Word = "jacket", Vietnamese = "áo khoác" },
                new Vocabulary { Word = "juice", Vietnamese = "nước ép" },
                new Vocabulary { Word = "join", Vietnamese = "tham gia" },
                new Vocabulary { Word = "key", Vietnamese = "chìa khóa" },
                new Vocabulary { Word = "kitchen", Vietnamese = "nhà bếp" },
                new Vocabulary { Word = "kind", Vietnamese = "tốt bụng" },
                new Vocabulary { Word = "lamp", Vietnamese = "đèn" },
                new Vocabulary { Word = "language", Vietnamese = "ngôn ngữ" },
                new Vocabulary { Word = "light", Vietnamese = "ánh sáng" },
                new Vocabulary { Word = "learn", Vietnamese = "học" },
                new Vocabulary { Word = "mountain", Vietnamese = "ngọn núi" },
                new Vocabulary { Word = "money", Vietnamese = "tiền" },
                new Vocabulary { Word = "music", Vietnamese = "âm nhạc" },
                new Vocabulary { Word = "market", Vietnamese = "chợ" },
                new Vocabulary { Word = "night", Vietnamese = "đêm" },
                new Vocabulary { Word = "name", Vietnamese = "tên" },
                new Vocabulary { Word = "nature", Vietnamese = "thiên nhiên" },
                new Vocabulary { Word = "ocean", Vietnamese = "đại dương" },
                new Vocabulary { Word = "open", Vietnamese = "mở" },
                new Vocabulary { Word = "orange", Vietnamese = "quả cam" },
                new Vocabulary { Word = "people", Vietnamese = "con người" },
                new Vocabulary { Word = "plant", Vietnamese = "cây" },
                new Vocabulary { Word = "peace", Vietnamese = "hòa bình" },
                new Vocabulary { Word = "phone", Vietnamese = "điện thoại" },
                new Vocabulary { Word = "question", Vietnamese = "câu hỏi" },
                new Vocabulary { Word = "quiet", Vietnamese = "yên tĩnh" },
                new Vocabulary { Word = "river", Vietnamese = "dòng sông" },
                new Vocabulary { Word = "road", Vietnamese = "đường" },
                new Vocabulary { Word = "rain", Vietnamese = "mưa" },
                new Vocabulary { Word = "room", Vietnamese = "phòng" },
                new Vocabulary { Word = "school", Vietnamese = "trường học" },
                new Vocabulary { Word = "sun", Vietnamese = "mặt trời" },
                new Vocabulary { Word = "stone", Vietnamese = "đá" },
                new Vocabulary { Word = "sister", Vietnamese = "chị/em gái" },
                new Vocabulary { Word = "sea", Vietnamese = "biển" },
                new Vocabulary { Word = "tree", Vietnamese = "cây" },
                new Vocabulary { Word = "time", Vietnamese = "thời gian" },
                new Vocabulary { Word = "travel", Vietnamese = "du lịch" },
                new Vocabulary { Word = "table", Vietnamese = "bàn" },
                new Vocabulary { Word = "umbrella", Vietnamese = "ô/dù" },
                new Vocabulary { Word = "university", Vietnamese = "đại học" },
                new Vocabulary { Word = "useful", Vietnamese = "hữu ích" },
                new Vocabulary { Word = "village", Vietnamese = "làng" },
                new Vocabulary { Word = "voice", Vietnamese = "giọng nói" },
                new Vocabulary { Word = "visit", Vietnamese = "thăm" },
                new Vocabulary { Word = "water", Vietnamese = "nước" },
                new Vocabulary { Word = "world", Vietnamese = "thế giới" },
                new Vocabulary { Word = "work", Vietnamese = "công việc" },
                new Vocabulary { Word = "window", Vietnamese = "cửa sổ" },
                new Vocabulary { Word = "xylophone", Vietnamese = "đàn xylophone" },
                new Vocabulary { Word = "yellow", Vietnamese = "màu vàng" },
                new Vocabulary { Word = "young", Vietnamese = "trẻ" },
                new Vocabulary { Word = "yard", Vietnamese = "sân" },
                new Vocabulary { Word = "zebra", Vietnamese = "ngựa vằn" },
                new Vocabulary { Word = "zero", Vietnamese = "không" },
                new Vocabulary { Word = "accept", Vietnamese = "chấp nhận" },
                new Vocabulary { Word = "build", Vietnamese = "xây dựng" },
                new Vocabulary { Word = "change", Vietnamese = "thay đổi" },
                new Vocabulary { Word = "develop", Vietnamese = "phát triển" },
                new Vocabulary { Word = "enjoy", Vietnamese = "thưởng thức" },
                new Vocabulary { Word = "finish", Vietnamese = "hoàn thành" },
                new Vocabulary { Word = "gather", Vietnamese = "tập hợp" },
                new Vocabulary { Word = "handle", Vietnamese = "xử lý" },
                new Vocabulary { Word = "imagine", Vietnamese = "tưởng tượng" }
            };

            var missing = vocabularies
                .Where(v => !string.IsNullOrWhiteSpace(v.Word) && !existingWords.Contains(v.Word))
                .GroupBy(v => v.Word, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            if (!missing.Any())
            {
                Console.WriteLine("No new vocabulary to seed");
                return;
            }

            context.Vocabularies.AddRange(missing);
            Console.WriteLine($"Queued {missing.Count} new vocabulary words for seeding");
        }
    }
}
