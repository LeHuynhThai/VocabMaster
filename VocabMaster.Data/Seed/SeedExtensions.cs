using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VocabMaster.Data.Seed
{
    public static class SeedExtensions
    {
        public static IApplicationBuilder SeedDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<AppDbContext>>();
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                Task.Run(() => SeedData.Initialize(services, logger)).Wait();

                logger.LogInformation("Database seeded successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
            }

            return app;
        }
    }
}