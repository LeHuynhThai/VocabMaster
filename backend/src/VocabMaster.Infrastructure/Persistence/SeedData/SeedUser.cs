using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VocabMaster.Domain.Entities;
using VocabMaster.Infrastructure.Persistence;

namespace VocabMaster.Infrastructure.Persistence.SeedData
{
    public class SeedUser
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                await context.Database.EnsureCreatedAsync();

                await SeedUserData(context);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task SeedUserData(ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync())
            {
                Console.WriteLine("Users already seeded");
                return;
            }

            var users = new List<User>();

            users.Add(new User
            {
                Name = "Admin",
                Password = BCrypt.Net.BCrypt.HashPassword("123"),
                Role = UserRole.Admin
            });

            for (int i = 1; i <= 10; i++)
            {
                users.Add(new User
                {
                    Name = $"User{i}",
                    Password = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = UserRole.User
                });
            }

            context.Users.AddRange(users);
            Console.WriteLine("Users seeded successfully");
        }
    }
}
