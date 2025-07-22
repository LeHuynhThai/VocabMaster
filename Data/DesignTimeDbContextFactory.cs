using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace VocabMaster.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Combine("API", "appsettings.json"), optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("AppDbContext");

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=localhost;Database=VocabMaster;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}