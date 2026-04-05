using Microsoft.EntityFrameworkCore;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;
using VocabMaster.Infrastructure.Persistence;

namespace VocabMaster.Infrastructure.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _context;

        public UserRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByName(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
        }

        public async Task<User?> GetById(int id)
        {
            return await _context.Users
                .Include(u => u.LearnedVocabularies)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> IsNameExist(string name)
        {
            return await _context.Users.AnyAsync(u => u.Name == name);
        }

        public async Task Add(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}