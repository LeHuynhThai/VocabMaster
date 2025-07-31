using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VocabMaster.Core.Entities;

namespace VocabMaster.Data.Seed
{
    public static class SeedQuizQuestions
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (await context.QuizQuestions.AnyAsync())
                return;

            var quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion { Word = "Honest", CorrectAnswer = "Trung thực", WrongAnswer1 = "Giàu có", WrongAnswer2 = "Lười biếng", WrongAnswer3 = "Bẩn thỉu" },
                new QuizQuestion { Word = "Dishonest", CorrectAnswer = "Không trung thực", WrongAnswer1 = "Vui vẻ", WrongAnswer2 = "Khỏe mạnh", WrongAnswer3 = "Đẹp trai" },
                new QuizQuestion { Word = "Equality", CorrectAnswer = "Bình đẳng", WrongAnswer1 = "Bất công", WrongAnswer2 = "Giàu sang", WrongAnswer3 = "Lười biếng" },
                new QuizQuestion { Word = "Choice", CorrectAnswer = "Lựa chọn", WrongAnswer1 = "Bắt buộc", WrongAnswer2 = "Bỏ qua", WrongAnswer3 = "Ghi chú" },
                new QuizQuestion { Word = "Note", CorrectAnswer = "Ghi chú", WrongAnswer1 = "Lựa chọn", WrongAnswer2 = "Bình đẳng", WrongAnswer3 = "Không trung thực" },
                new QuizQuestion { Word = "Brave", CorrectAnswer = "Dũng cảm", WrongAnswer1 = "Nhút nhát", WrongAnswer2 = "Lười biếng", WrongAnswer3 = "Bất công" },
                new QuizQuestion { Word = "Lazy", CorrectAnswer = "Lười biếng", WrongAnswer1 = "Chăm chỉ", WrongAnswer2 = "Trung thực", WrongAnswer3 = "Dũng cảm" },
                new QuizQuestion { Word = "Wealthy", CorrectAnswer = "Giàu có", WrongAnswer1 = "Nghèo", WrongAnswer2 = "Bình đẳng", WrongAnswer3 = "Không trung thực" },
                new QuizQuestion { Word = "Poor", CorrectAnswer = "Nghèo", WrongAnswer1 = "Giàu có", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Beautiful", CorrectAnswer = "Đẹp", WrongAnswer1 = "Xấu", WrongAnswer2 = "Bình đẳng", WrongAnswer3 = "Không trung thực" },
                new QuizQuestion { Word = "Ugly", CorrectAnswer = "Xấu", WrongAnswer1 = "Đẹp", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Strong", CorrectAnswer = "Khỏe mạnh", WrongAnswer1 = "Yếu", WrongAnswer2 = "Lười biếng", WrongAnswer3 = "Không trung thực" },
                new QuizQuestion { Word = "Weak", CorrectAnswer = "Yếu", WrongAnswer1 = "Khỏe mạnh", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Happy", CorrectAnswer = "Vui vẻ", WrongAnswer1 = "Buồn", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Sad", CorrectAnswer = "Buồn", WrongAnswer1 = "Vui vẻ", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Fair", CorrectAnswer = "Công bằng", WrongAnswer1 = "Bất công", WrongAnswer2 = "Lười biếng", WrongAnswer3 = "Không trung thực" },
                new QuizQuestion { Word = "Unfair", CorrectAnswer = "Bất công", WrongAnswer1 = "Công bằng", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Polite", CorrectAnswer = "Lịch sự", WrongAnswer1 = "Thô lỗ", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Rude", CorrectAnswer = "Thô lỗ", WrongAnswer1 = "Lịch sự", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Clean", CorrectAnswer = "Sạch sẽ", WrongAnswer1 = "Bẩn thỉu", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Dirty", CorrectAnswer = "Bẩn thỉu", WrongAnswer1 = "Sạch sẽ", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Early", CorrectAnswer = "Sớm", WrongAnswer1 = "Muộn", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Late", CorrectAnswer = "Muộn", WrongAnswer1 = "Sớm", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Careful", CorrectAnswer = "Cẩn thận", WrongAnswer1 = "Bất cẩn", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Careless", CorrectAnswer = "Bất cẩn", WrongAnswer1 = "Cẩn thận", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Friendly", CorrectAnswer = "Thân thiện", WrongAnswer1 = "Thù địch", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Hostile", CorrectAnswer = "Thù địch", WrongAnswer1 = "Thân thiện", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Patient", CorrectAnswer = "Kiên nhẫn", WrongAnswer1 = "Nóng vội", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Impatient", CorrectAnswer = "Nóng vội", WrongAnswer1 = "Kiên nhẫn", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Generous", CorrectAnswer = "Hào phóng", WrongAnswer1 = "Keo kiệt", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Stingy", CorrectAnswer = "Keo kiệt", WrongAnswer1 = "Hào phóng", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Intelligent", CorrectAnswer = "Thông minh", WrongAnswer1 = "Ngu dốt", WrongAnswer2 = "Lười biếng", WrongAnswer3 = "Bất công" },
                new QuizQuestion { Word = "Stupid", CorrectAnswer = "Ngu dốt", WrongAnswer1 = "Thông minh", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Tall", CorrectAnswer = "Cao", WrongAnswer1 = "Thấp", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Short", CorrectAnswer = "Thấp", WrongAnswer1 = "Cao", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Fast", CorrectAnswer = "Nhanh", WrongAnswer1 = "Chậm", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Slow", CorrectAnswer = "Chậm", WrongAnswer1 = "Nhanh", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Big", CorrectAnswer = "To lớn", WrongAnswer1 = "Nhỏ bé", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Small", CorrectAnswer = "Nhỏ bé", WrongAnswer1 = "To lớn", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Hot", CorrectAnswer = "Nóng", WrongAnswer1 = "Lạnh", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Cold", CorrectAnswer = "Lạnh", WrongAnswer1 = "Nóng", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Old", CorrectAnswer = "Già", WrongAnswer1 = "Trẻ", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Young", CorrectAnswer = "Trẻ", WrongAnswer1 = "Già", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Expensive", CorrectAnswer = "Đắt", WrongAnswer1 = "Rẻ", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Cheap", CorrectAnswer = "Rẻ", WrongAnswer1 = "Đắt", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Easy", CorrectAnswer = "Dễ dàng", WrongAnswer1 = "Khó khăn", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Difficult", CorrectAnswer = "Khó khăn", WrongAnswer1 = "Dễ dàng", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Interesting", CorrectAnswer = "Thú vị", WrongAnswer1 = "Nhàm chán", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Boring", CorrectAnswer = "Nhàm chán", WrongAnswer1 = "Thú vị", WrongAnswer2 = "Giàu có", WrongAnswer3 = "Lựa chọn" },
                new QuizQuestion { Word = "Healthy", CorrectAnswer = "Khỏe mạnh", WrongAnswer1 = "Bệnh tật", WrongAnswer2 = "Không trung thực", WrongAnswer3 = "Bình đẳng" },
                new QuizQuestion { Word = "Sick", CorrectAnswer = "Bệnh tật", WrongAnswer1 = "Khỏe mạnh", WrongAnswer2 = "Dũng cảm", WrongAnswer3 = "Lựa chọn" }
            };

            await context.QuizQuestions.AddRangeAsync(quizQuestions);
            await context.SaveChangesAsync();
        }
    }
}