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
    public class SeedQuizQuestion
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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

        private static async Task SeedQuizQuestionData(AppDbContext context)
        {
            if (await context.QuizQuestions.AnyAsync())
            {
                Console.WriteLine("Quiz questions already seeded");
                return;
            }

            var quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Word = "beautiful",
                    CorrectAnswer = "đẹp",
                    WrongAnswer1 = "xấu",
                    WrongAnswer2 = "lớn",
                    WrongAnswer3 = "nhỏ"
                },
                new QuizQuestion
                {
                    Word = "happy",
                    CorrectAnswer = "vui vẻ",
                    WrongAnswer1 = "buồn",
                    WrongAnswer2 = "tức giận",
                    WrongAnswer3 = "lo lắng"
                },
                new QuizQuestion
                {
                    Word = "house",
                    CorrectAnswer = "ngôi nhà",
                    WrongAnswer1 = "xe hơi",
                    WrongAnswer2 = "cây cối",
                    WrongAnswer3 = "con chó"
                },
                new QuizQuestion
                {
                    Word = "water",
                    CorrectAnswer = "nước",
                    WrongAnswer1 = "lửa",
                    WrongAnswer2 = "đất",
                    WrongAnswer3 = "không khí"
                },
                new QuizQuestion
                {
                    Word = "book",
                    CorrectAnswer = "cuốn sách",
                    WrongAnswer1 = "bút chì",
                    WrongAnswer2 = "giấy",
                    WrongAnswer3 = "cục tẩy"
                },
                new QuizQuestion
                {
                    Word = "friend",
                    CorrectAnswer = "bạn bè",
                    WrongAnswer1 = "kẻ thù",
                    WrongAnswer2 = "người lạ",
                    WrongAnswer3 = "giáo viên"
                },
                new QuizQuestion
                {
                    Word = "food",
                    CorrectAnswer = "thức ăn",
                    WrongAnswer1 = "nước uống",
                    WrongAnswer2 = "quần áo",
                    WrongAnswer3 = "đồ chơi"
                },
                new QuizQuestion
                {
                    Word = "school",
                    CorrectAnswer = "trường học",
                    WrongAnswer1 = "bệnh viện",
                    WrongAnswer2 = "cửa hàng",
                    WrongAnswer3 = "nhà ga"
                },
                new QuizQuestion
                {
                    Word = "family",
                    CorrectAnswer = "gia đình",
                    WrongAnswer1 = "bạn bè",
                    WrongAnswer2 = "đồng nghiệp",
                    WrongAnswer3 = "hàng xóm"
                },
                new QuizQuestion
                {
                    Word = "money",
                    CorrectAnswer = "tiền bạc",
                    WrongAnswer1 = "vàng",
                    WrongAnswer2 = "kim cương",
                    WrongAnswer3 = "bạc"
                },
                new QuizQuestion
                {
                    Word = "time",
                    CorrectAnswer = "thời gian",
                    WrongAnswer1 = "không gian",
                    WrongAnswer2 = "khoảng cách",
                    WrongAnswer3 = "tốc độ"
                },
                new QuizQuestion
                {
                    Word = "love",
                    CorrectAnswer = "tình yêu",
                    WrongAnswer1 = "thù hận",
                    WrongAnswer2 = "ghét bỏ",
                    WrongAnswer3 = "thờ ơ"
                },
                new QuizQuestion
                {
                    Word = "work",
                    CorrectAnswer = "công việc",
                    WrongAnswer1 = "nghỉ ngơi",
                    WrongAnswer2 = "vui chơi",
                    WrongAnswer3 = "học tập"
                },
                new QuizQuestion
                {
                    Word = "home",
                    CorrectAnswer = "nhà",
                    WrongAnswer1 = "văn phòng",
                    WrongAnswer2 = "trường học",
                    WrongAnswer3 = "bệnh viện"
                },
                new QuizQuestion
                {
                    Word = "life",
                    CorrectAnswer = "cuộc sống",
                    WrongAnswer1 = "cái chết",
                    WrongAnswer2 = "giấc ngủ",
                    WrongAnswer3 = "giấc mơ"
                },
                new QuizQuestion
                {
                    Word = "world",
                    CorrectAnswer = "thế giới",
                    WrongAnswer1 = "vũ trụ",
                    WrongAnswer2 = "hành tinh",
                    WrongAnswer3 = "thiên hà"
                },
                new QuizQuestion
                {
                    Word = "people",
                    CorrectAnswer = "con người",
                    WrongAnswer1 = "động vật",
                    WrongAnswer2 = "thực vật",
                    WrongAnswer3 = "vi khuẩn"
                },
                new QuizQuestion
                {
                    Word = "child",
                    CorrectAnswer = "đứa trẻ",
                    WrongAnswer1 = "người lớn",
                    WrongAnswer2 = "người già",
                    WrongAnswer3 = "thanh niên"
                },
                new QuizQuestion
                {
                    Word = "mother",
                    CorrectAnswer = "mẹ",
                    WrongAnswer1 = "bố",
                    WrongAnswer2 = "anh trai",
                    WrongAnswer3 = "chị gái"
                },
                new QuizQuestion
                {
                    Word = "father",
                    CorrectAnswer = "bố",
                    WrongAnswer1 = "mẹ",
                    WrongAnswer2 = "ông nội",
                    WrongAnswer3 = "bà ngoại"
                }
            };

            context.QuizQuestions.AddRange(quizQuestions);
            Console.WriteLine($"Seeded {quizQuestions.Count} quiz questions successfully");
        }
    }
}
