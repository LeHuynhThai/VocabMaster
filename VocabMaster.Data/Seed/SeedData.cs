using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;

namespace VocabMaster.Data.Seed
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, ILogger logger)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            await context.Database.EnsureCreatedAsync();

            try
            {
                if (!context.Users.Any())
                {
                    logger.LogInformation("Seeding users...");
                    await SeedUsers(context);
                }

                if (!context.QuizQuestions.Any())
                {
                    logger.LogInformation("Seeding quiz questions...");
                    await SeedQuizQuestions.SeedAsync(serviceProvider);
                }

                logger.LogInformation("Seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private static async Task SeedUsers(AppDbContext context)
        {
            var users = new List<User>();

            for (int i = 1; i <= 50; i++)
            {
                var user = new User
                {
                    Name = $"user{i}",
                    Password = BCrypt.Net.BCrypt.HashPassword($"123"),
                    Role = UserRole.User
                };

                users.Add(user);
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }
}