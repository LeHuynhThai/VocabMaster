using Microsoft.EntityFrameworkCore;
using VocabMaster.Core.Entities;
using System;

namespace VocabMaster.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LearnedWord> LearnedVocabularies { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Create composite index for Word and UserId
            builder.Entity<LearnedWord>()
                .HasIndex(lv => new { lv.Word, lv.UserId })
                .IsUnique();

            // Configure relationship between User and LearnedVocabulary
            builder.Entity<User>()
                .HasMany(u => u.LearnedVocabularies)
                .WithOne(lv => lv.User)
                .HasForeignKey(lv => lv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
