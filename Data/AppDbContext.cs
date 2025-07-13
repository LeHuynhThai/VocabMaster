using Microsoft.EntityFrameworkCore;
using VocabMaster.Entities;
using VocabMaster.Models;

namespace VocabMaster.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }
    }
}
