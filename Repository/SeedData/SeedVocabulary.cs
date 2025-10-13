using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Repository;
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
            };

            context.Vocabularies.AddRange(vocabularies);
            Console.WriteLine($"Seeded {vocabularies.Count} vocabulary words successfully");
        }
    }
}
