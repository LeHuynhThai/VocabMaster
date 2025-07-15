using Microsoft.EntityFrameworkCore;
using VocabMaster.Entities;

namespace VocabMaster.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vocabulary>()
                .HasIndex(v => v.Word)
                .IsUnique();
        }
    }
}
