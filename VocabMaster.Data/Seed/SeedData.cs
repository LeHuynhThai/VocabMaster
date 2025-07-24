using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VocabMaster.Core.Entities;

namespace VocabMaster.Data.Seed
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, ILogger logger)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            try
            {
                // Seed users if there are none
                if (!context.Users.Any())
                {
                    logger.LogInformation("Seeding users...");
                    await SeedUsers(context);
                }

                // Seed vocabularies if there are none
                if (!context.Vocabularies.Any())
                {
                    logger.LogInformation("Seeding vocabularies...");
                    await SeedVocabularies(context);
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

            // Create 50 basic users
            for (int i = 1; i <= 50; i++)
            {
                var user = new User
                {
                    Name = $"user{i}",
                    // Hash the password using BCrypt
                    Password = BCrypt.Net.BCrypt.HashPassword($"123"), // password is 123
                    Role = UserRole.User
                };

                users.Add(user);
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        private static async Task SeedVocabularies(AppDbContext context)
        {
            // Load basic vocabulary words from the BasicVocabulary.cs file
            var basicWords = BasicVocabulary.Words;

            var vocabularies = basicWords.Select(word => new Vocabulary
            {
                Word = word
            }).ToList();

            await context.Vocabularies.AddRangeAsync(vocabularies);
            await context.SaveChangesAsync();
        }
    }
} 