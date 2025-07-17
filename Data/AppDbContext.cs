using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Models;

namespace VocabMaster.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<LearnedVocabulary> LearnedVocabularies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Tạo composite index cho Word và UserId
            builder.Entity<LearnedVocabulary>()
                .HasIndex(lv => new { lv.Word, lv.UserId })
                .IsUnique();
        }
    }
}
