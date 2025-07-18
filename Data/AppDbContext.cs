using Microsoft.EntityFrameworkCore;
using VocabMaster.Entities;

namespace VocabMaster.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LearnedVocabulary> LearnedVocabularies { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Tạo composite index cho Word và UserId
            builder.Entity<LearnedVocabulary>()
                .HasIndex(lv => new { lv.Word, lv.UserId })
                .IsUnique();

            // Cấu hình relationship giữa User và LearnedVocabulary
            builder.Entity<User>()
                .HasMany(u => u.LearnedVocabularies)
                .WithOne(lv => lv.User)
                .HasForeignKey(lv => lv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
