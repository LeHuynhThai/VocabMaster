using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace VocabMaster.Data.Seed
{
    public static class SeedExtensions
    {
        /// <summary>
        /// Adds database seeding to the application startup process.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder</returns>
        public static async Task<IApplicationBuilder> SeedDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseSeeder");

            try
            {
                await SeedData.Initialize(services, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }

            return app;
        }
    }
} 