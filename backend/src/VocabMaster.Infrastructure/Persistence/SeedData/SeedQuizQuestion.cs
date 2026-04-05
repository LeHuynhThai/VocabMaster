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
    public class SeedQuizQuestion
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                await context.Database.EnsureCreatedAsync();

                await SeedQuizQuestionData(context);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task SeedQuizQuestionData(ApplicationDbContext context)
        {
            // Load existing words to avoid inserting duplicates.
            var existingWordsList = await context.QuizQuestions
                .Select(q => q.Word)
                .ToListAsync();
            var existingWords = new HashSet<string>(existingWordsList, StringComparer.OrdinalIgnoreCase);

            var allQuestions = new List<QuizQuestion>
            {
                new QuizQuestion { Word = "beautiful", CorrectAnswer = "đẹp", WrongAnswer1 = "xấu", WrongAnswer2 = "lớn", WrongAnswer3 = "nhỏ" },
                new QuizQuestion { Word = "happy", CorrectAnswer = "vui vẻ", WrongAnswer1 = "buồn", WrongAnswer2 = "tức giận", WrongAnswer3 = "lo lắng" },
                new QuizQuestion { Word = "house", CorrectAnswer = "ngôi nhà", WrongAnswer1 = "xe hơi", WrongAnswer2 = "cây cối", WrongAnswer3 = "con chó" },
                new QuizQuestion { Word = "water", CorrectAnswer = "nước", WrongAnswer1 = "lửa", WrongAnswer2 = "đất", WrongAnswer3 = "không khí" },
                new QuizQuestion { Word = "book", CorrectAnswer = "cuốn sách", WrongAnswer1 = "bút chì", WrongAnswer2 = "giấy", WrongAnswer3 = "cục tẩy" },
                new QuizQuestion { Word = "friend", CorrectAnswer = "bạn bè", WrongAnswer1 = "kẻ thù", WrongAnswer2 = "người lạ", WrongAnswer3 = "giáo viên" },
                new QuizQuestion { Word = "food", CorrectAnswer = "thức ăn", WrongAnswer1 = "nước uống", WrongAnswer2 = "quần áo", WrongAnswer3 = "đồ chơi" },
                new QuizQuestion { Word = "school", CorrectAnswer = "trường học", WrongAnswer1 = "bệnh viện", WrongAnswer2 = "cửa hàng", WrongAnswer3 = "nhà ga" },
                new QuizQuestion { Word = "family", CorrectAnswer = "gia đình", WrongAnswer1 = "bạn bè", WrongAnswer2 = "đồng nghiệp", WrongAnswer3 = "hàng xóm" },
                new QuizQuestion { Word = "money", CorrectAnswer = "tiền bạc", WrongAnswer1 = "vàng", WrongAnswer2 = "kim cương", WrongAnswer3 = "bạc" },
                new QuizQuestion { Word = "time", CorrectAnswer = "thời gian", WrongAnswer1 = "không gian", WrongAnswer2 = "khoảng cách", WrongAnswer3 = "tốc độ" },
                new QuizQuestion { Word = "love", CorrectAnswer = "tình yêu", WrongAnswer1 = "thù hận", WrongAnswer2 = "ghét bỏ", WrongAnswer3 = "thờ ơ" },
                new QuizQuestion { Word = "work", CorrectAnswer = "công việc", WrongAnswer1 = "nghỉ ngơi", WrongAnswer2 = "vui chơi", WrongAnswer3 = "học tập" },
                new QuizQuestion { Word = "home", CorrectAnswer = "nhà", WrongAnswer1 = "văn phòng", WrongAnswer2 = "trường học", WrongAnswer3 = "bệnh viện" },
                new QuizQuestion { Word = "life", CorrectAnswer = "cuộc sống", WrongAnswer1 = "cái chết", WrongAnswer2 = "giấc ngủ", WrongAnswer3 = "giấc mơ" },
                new QuizQuestion { Word = "world", CorrectAnswer = "thế giới", WrongAnswer1 = "vũ trụ", WrongAnswer2 = "hành tinh", WrongAnswer3 = "thiên hà" },
                new QuizQuestion { Word = "people", CorrectAnswer = "con người", WrongAnswer1 = "động vật", WrongAnswer2 = "thực vật", WrongAnswer3 = "vi khuẩn" },
                new QuizQuestion { Word = "child", CorrectAnswer = "đứa trẻ", WrongAnswer1 = "người lớn", WrongAnswer2 = "người già", WrongAnswer3 = "thanh niên" },
                new QuizQuestion { Word = "mother", CorrectAnswer = "mẹ", WrongAnswer1 = "bố", WrongAnswer2 = "anh trai", WrongAnswer3 = "chị gái" },
                new QuizQuestion { Word = "father", CorrectAnswer = "bố", WrongAnswer1 = "mẹ", WrongAnswer2 = "ông nội", WrongAnswer3 = "bà ngoại" },
                new QuizQuestion { Word = "city", CorrectAnswer = "thành phố", WrongAnswer1 = "làng", WrongAnswer2 = "thị trấn", WrongAnswer3 = "đảo" },
                new QuizQuestion { Word = "car", CorrectAnswer = "xe hơi", WrongAnswer1 = "xe đạp", WrongAnswer2 = "máy bay", WrongAnswer3 = "tàu hỏa" },
                new QuizQuestion { Word = "cat", CorrectAnswer = "con mèo", WrongAnswer1 = "con chó", WrongAnswer2 = "con chim", WrongAnswer3 = "con bò" },
                new QuizQuestion { Word = "dog", CorrectAnswer = "con chó", WrongAnswer1 = "con mèo", WrongAnswer2 = "con ngựa", WrongAnswer3 = "con cá" },
                new QuizQuestion { Word = "sun", CorrectAnswer = "mặt trời", WrongAnswer1 = "mặt trăng", WrongAnswer2 = "sao", WrongAnswer3 = "đám mây" },
                new QuizQuestion { Word = "moon", CorrectAnswer = "mặt trăng", WrongAnswer1 = "mặt trời", WrongAnswer2 = "sao", WrongAnswer3 = "đất" },
                new QuizQuestion { Word = "star", CorrectAnswer = "ngôi sao", WrongAnswer1 = "mặt trăng", WrongAnswer2 = "mặt trời", WrongAnswer3 = "hành tinh" },
                new QuizQuestion { Word = "tree", CorrectAnswer = "cây", WrongAnswer1 = "bông hoa", WrongAnswer2 = "lá", WrongAnswer3 = "cỏ" },
                new QuizQuestion { Word = "river", CorrectAnswer = "sông", WrongAnswer1 = "biển", WrongAnswer2 = "hồ", WrongAnswer3 = "suối" },
                new QuizQuestion { Word = "mountain", CorrectAnswer = "ngọn núi", WrongAnswer1 = "đồi", WrongAnswer2 = "thung lũng", WrongAnswer3 = "cao nguyên" },
                new QuizQuestion { Word = "glass", CorrectAnswer = "cốc thủy tinh", WrongAnswer1 = "gốm", WrongAnswer2 = "nhựa", WrongAnswer3 = "giấy" },
                new QuizQuestion { Word = "phone", CorrectAnswer = "điện thoại", WrongAnswer1 = "máy tính", WrongAnswer2 = "đồng hồ", WrongAnswer3 = "ti vi" },
                new QuizQuestion { Word = "computer", CorrectAnswer = "máy tính", WrongAnswer1 = "điện thoại", WrongAnswer2 = "máy giặt", WrongAnswer3 = "bếp" },
                new QuizQuestion { Word = "key", CorrectAnswer = "chìa khóa", WrongAnswer1 = "bản đồ", WrongAnswer2 = "cái nón", WrongAnswer3 = "bút" },
                new QuizQuestion { Word = "door", CorrectAnswer = "cửa", WrongAnswer1 = "cửa sổ", WrongAnswer2 = "tường", WrongAnswer3 = "mái nhà" },
                new QuizQuestion { Word = "window", CorrectAnswer = "cửa sổ", WrongAnswer1 = "cửa", WrongAnswer2 = "mái nhà", WrongAnswer3 = "hành lang" },
                new QuizQuestion { Word = "chair", CorrectAnswer = "ghế", WrongAnswer1 = "bàn", WrongAnswer2 = "tủ", WrongAnswer3 = "giường" },
                new QuizQuestion { Word = "table", CorrectAnswer = "bàn", WrongAnswer1 = "ghế", WrongAnswer2 = "kệ", WrongAnswer3 = "tủ" },
                new QuizQuestion { Word = "shirt", CorrectAnswer = "áo sơ mi", WrongAnswer1 = "quần", WrongAnswer2 = "giày", WrongAnswer3 = "mũ" },
                new QuizQuestion { Word = "shoe", CorrectAnswer = "giày", WrongAnswer1 = "tất", WrongAnswer2 = "dép", WrongAnswer3 = "vớ" },
                new QuizQuestion { Word = "road", CorrectAnswer = "đường", WrongAnswer1 = "đường sắt", WrongAnswer2 = "cầu", WrongAnswer3 = "vỉa hè" },
                new QuizQuestion { Word = "island", CorrectAnswer = "đảo", WrongAnswer1 = "bán đảo", WrongAnswer2 = "lục địa", WrongAnswer3 = "hòn" },
                new QuizQuestion { Word = "garden", CorrectAnswer = "vườn", WrongAnswer1 = "công viên", WrongAnswer2 = "sân", WrongAnswer3 = "đồng cỏ" },
                new QuizQuestion { Word = "flower", CorrectAnswer = "bông hoa", WrongAnswer1 = "lá", WrongAnswer2 = "cây", WrongAnswer3 = "quả" },
                new QuizQuestion { Word = "rain", CorrectAnswer = "mưa", WrongAnswer1 = "tuyết", WrongAnswer2 = "gió", WrongAnswer3 = "nắng" },
                new QuizQuestion { Word = "snow", CorrectAnswer = "tuyết", WrongAnswer1 = "mưa", WrongAnswer2 = "băng", WrongAnswer3 = "sương mù" },
                new QuizQuestion { Word = "wind", CorrectAnswer = "gió", WrongAnswer1 = "mưa", WrongAnswer2 = "nắng", WrongAnswer3 = "bão" },
                new QuizQuestion { Word = "beach", CorrectAnswer = "bãi biển", WrongAnswer1 = "bờ hồ", WrongAnswer2 = "đầm phá", WrongAnswer3 = "cảng" },
                new QuizQuestion { Word = "forest", CorrectAnswer = "rừng", WrongAnswer1 = "vườn", WrongAnswer2 = "đồng cỏ", WrongAnswer3 = "đồi" },
                new QuizQuestion { Word = "lamp", CorrectAnswer = "đèn", WrongAnswer1 = "nến", WrongAnswer2 = "đèn pin", WrongAnswer3 = "bóng đèn" }
            };

            // Filter out words that already exist in the DB
            var newQuestions = allQuestions
                .Where(q => !string.IsNullOrWhiteSpace(q.Word) && !existingWords.Contains(q.Word))
                .GroupBy(q => q.Word, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            if (!newQuestions.Any())
            {
                Console.WriteLine("No new quiz questions to seed");
                return;
            }

            context.QuizQuestions.AddRange(newQuestions);
            Console.WriteLine($"Queued {newQuestions.Count} new quiz questions for seeding");
        }
    }
}
