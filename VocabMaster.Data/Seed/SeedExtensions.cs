using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;

using Microsoft.AspNetCore.Builder;
using System.IO; // Added for File operations

namespace VocabMaster.Data.Seed
{
    public static class SeedExtensions
    {
        public static IApplicationBuilder SeedDatabaseAsync(this IApplicationBuilder app)
        {
            // Create a scope to resolve services
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<AppDbContext>>();
            
            // Run the initialization in a try/catch block
            try
            {
                // Get the context instance
                var context = services.GetRequiredService<AppDbContext>();
                
                // Run initialization
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