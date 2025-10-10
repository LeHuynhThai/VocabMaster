using Microsoft.EntityFrameworkCore;
using VocabMaster.Core.Entities;

namespace VocabMaster.Data.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _context;
        
        public UserRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByName(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
        }
        public async Task<User> GetById(int id)
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

        public async Task<User> ValidateUser(string name, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
            return user;
        }
    }
}