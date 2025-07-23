using Microsoft.EntityFrameworkCore;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Data;

namespace VocabMaster.Data.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _context;

        public UserRepo(AppDbContext context)
        {
            _context = context;
        }

        // get user by name
        public async Task<User> GetByName(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Name == name); // find
        }

        // check if user name exists
        public async Task<bool> IsNameExist(string name)
        {
            return await _context.Users.AnyAsync(u => u.Name == name);
        }

        // add user
        public async Task Add(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        // validate user
        public async Task<User> ValidateUser(string name, string password)
        {
            // Get user by name
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == name);

            // If user not found or password is incorrect
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }

            return user;
        }
    }
}