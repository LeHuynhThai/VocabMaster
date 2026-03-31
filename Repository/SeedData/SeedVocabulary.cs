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
                new Vocabulary { Word = "assemble", Vietnamese = "lắp ráp, tập hợp" },
                new Vocabulary { Word = "assess", Vietnamese = "đánh giá" },
                new Vocabulary { Word = "assign", Vietnamese = "phân công, giao nhiệm vụ" },
                new Vocabulary { Word = "assume", Vietnamese = "giả định, đảm nhận" },
                new Vocabulary { Word = "attain", Vietnamese = "đạt được" },
                new Vocabulary { Word = "attribute", Vietnamese = "thuộc tính, quy cho" },
                new Vocabulary { Word = "available", Vietnamese = "có sẵn, sẵn có" },
                new Vocabulary { Word = "aware", Vietnamese = "nhận thức" },
                new Vocabulary { Word = "benefit", Vietnamese = "lợi ích" },
                new Vocabulary { Word = "capable", Vietnamese = "có khả năng" },
                new Vocabulary { Word = "challenge", Vietnamese = "thách thức" },
                new Vocabulary { Word = "collaborate", Vietnamese = "hợp tác" },
                new Vocabulary { Word = "commit", Vietnamese = "cam kết" },
                new Vocabulary { Word = "communicate", Vietnamese = "giao tiếp" },
                new Vocabulary { Word = "compare", Vietnamese = "so sánh" },
                new Vocabulary { Word = "compete", Vietnamese = "cạnh tranh" },
                new Vocabulary { Word = "complex", Vietnamese = "phức tạp" },
                new Vocabulary { Word = "compose", Vietnamese = "soạn, cấu tạo" },
                new Vocabulary { Word = "conduct", Vietnamese = "tiến hành, hành vi" },
                new Vocabulary { Word = "confirm", Vietnamese = "xác nhận" },
                new Vocabulary { Word = "contribute", Vietnamese = "đóng góp" },
                new Vocabulary { Word = "convert", Vietnamese = "chuyển đổi" },
                new Vocabulary { Word = "criteria", Vietnamese = "tiêu chí" },
                new Vocabulary { Word = "data", Vietnamese = "dữ liệu" },
                new Vocabulary { Word = "declare", Vietnamese = "tuyên bố" },
                new Vocabulary { Word = "define", Vietnamese = "định nghĩa" },
                new Vocabulary { Word = "demonstrate", Vietnamese = "chứng minh, thể hiện" },
                new Vocabulary { Word = "derive", Vietnamese = "bắt nguồn từ" },
                new Vocabulary { Word = "design", Vietnamese = "thiết kế" },
                new Vocabulary { Word = "determine", Vietnamese = "xác định" },
                new Vocabulary { Word = "develop", Vietnamese = "phát triển" },
                new Vocabulary { Word = "differentiate", Vietnamese = "phân biệt" },
            };

            context.Vocabularies.AddRange(vocabularies);
            Console.WriteLine($"Seeded {vocabularies.Count} vocabulary words successfully");
        }
    }
}
