using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VocabMaster.Core.Entities;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Repository.SeedData
{
    public class SeedVocabulary
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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

        private static async Task SeedVocabularyData(AppDbContext context)
        {
            if (await context.Vocabularies.AnyAsync())
            {
                Console.WriteLine("Vocabulary already seeded");
                return;
            }

            var vocabularies = new List<Vocabulary>
            {
                new Vocabulary { Word = "abundant", Vietnamese = "dồi dào, phong phú" },
                new Vocabulary { Word = "accomplish", Vietnamese = "hoàn thành, đạt được" },
                new Vocabulary { Word = "accurate", Vietnamese = "chính xác, đúng đắn" },
                new Vocabulary { Word = "achieve", Vietnamese = "đạt được, thành công" },
                new Vocabulary { Word = "acquire", Vietnamese = "có được, thu thập" },
                new Vocabulary { Word = "adequate", Vietnamese = "đầy đủ, thích hợp" },
                new Vocabulary { Word = "advance", Vietnamese = "tiến bộ, tiến lên" },
                new Vocabulary { Word = "advantage", Vietnamese = "lợi thế, ưu điểm" },
                new Vocabulary { Word = "analyze", Vietnamese = "phân tích, nghiên cứu" },
                new Vocabulary { Word = "appreciate", Vietnamese = "đánh giá cao, cảm kích" },
                new Vocabulary { Word = "approach", Vietnamese = "tiếp cận, phương pháp" },
                new Vocabulary { Word = "appropriate", Vietnamese = "thích hợp, phù hợp" },
                new Vocabulary { Word = "available", Vietnamese = "có sẵn, có thể dùng" },
                new Vocabulary { Word = "benefit", Vietnamese = "lợi ích, phúc lợi" },
                new Vocabulary { Word = "capable", Vietnamese = "có khả năng, đủ năng lực" },
                new Vocabulary { Word = "challenge", Vietnamese = "thách thức, khó khăn" },
                new Vocabulary { Word = "character", Vietnamese = "tính cách, nhân vật" },
                new Vocabulary { Word = "circumstance", Vietnamese = "hoàn cảnh, tình huống" },
                new Vocabulary { Word = "commitment", Vietnamese = "cam kết, sự tận tâm" },
                new Vocabulary { Word = "communication", Vietnamese = "giao tiếp, truyền thông" },
                new Vocabulary { Word = "community", Vietnamese = "cộng đồng, xã hội" },
                new Vocabulary { Word = "comprehensive", Vietnamese = "toàn diện, bao quát" },
                new Vocabulary { Word = "concept", Vietnamese = "khái niệm, ý tưởng" },
                new Vocabulary { Word = "confidence", Vietnamese = "tự tin, tin tưởng" },
                new Vocabulary { Word = "consider", Vietnamese = "xem xét, cân nhắc" },
                new Vocabulary { Word = "consistent", Vietnamese = "nhất quán, kiên định" },
                new Vocabulary { Word = "contribute", Vietnamese = "đóng góp, góp phần" },
                new Vocabulary { Word = "cooperation", Vietnamese = "hợp tác, cộng tác" },
                new Vocabulary { Word = "creative", Vietnamese = "sáng tạo, có óc tưởng tượng" },
                new Vocabulary { Word = "culture", Vietnamese = "văn hóa, nền văn minh" },
                new Vocabulary { Word = "decision", Vietnamese = "quyết định, sự lựa chọn" },
                new Vocabulary { Word = "demonstrate", Vietnamese = "chứng minh, trình bày" },
                new Vocabulary { Word = "develop", Vietnamese = "phát triển, mở rộng" },
                new Vocabulary { Word = "different", Vietnamese = "khác biệt, đa dạng" },
                new Vocabulary { Word = "effective", Vietnamese = "hiệu quả, có tác dụng" },
                new Vocabulary { Word = "efficient", Vietnamese = "hiệu suất cao, tiết kiệm" },
                new Vocabulary { Word = "encourage", Vietnamese = "khuyến khích, động viên" },
                new Vocabulary { Word = "environment", Vietnamese = "môi trường, hoàn cảnh" },
                new Vocabulary { Word = "essential", Vietnamese = "cần thiết, cơ bản" },
                new Vocabulary { Word = "experience", Vietnamese = "kinh nghiệm, trải nghiệm" },
                new Vocabulary { Word = "expertise", Vietnamese = "chuyên môn, kỹ năng" },
                new Vocabulary { Word = "flexible", Vietnamese = "linh hoạt, dễ thích ứng" },
                new Vocabulary { Word = "function", Vietnamese = "chức năng, hoạt động" },
                new Vocabulary { Word = "fundamental", Vietnamese = "cơ bản, nền tảng" },
                new Vocabulary { Word = "generate", Vietnamese = "tạo ra, sinh ra" },
                new Vocabulary { Word = "implement", Vietnamese = "thực hiện, áp dụng" },
                new Vocabulary { Word = "important", Vietnamese = "quan trọng, có ý nghĩa" },
                new Vocabulary { Word = "improve", Vietnamese = "cải thiện, nâng cao" },
                new Vocabulary { Word = "individual", Vietnamese = "cá nhân, riêng lẻ" },
                new Vocabulary { Word = "innovative", Vietnamese = "đổi mới, sáng tạo" },
                new Vocabulary { Word = "knowledge", Vietnamese = "kiến thức, hiểu biết" }
            };

            context.Vocabularies.AddRange(vocabularies);
            Console.WriteLine($"Seeded {vocabularies.Count} vocabulary words successfully");
        }
    }
}
