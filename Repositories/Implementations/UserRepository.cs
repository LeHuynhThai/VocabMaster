using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Entities;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context; // DbContext

    // Constructor
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    // Get user by name
    public async Task<User> GetByNameAsync(string name)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Name == name); // Get user by name
    }

    // Check if user name exists
    public async Task<bool> IsNameExistsAsync(string name)
    {
        return await _context.Users.AnyAsync(u => u.Name == name); // Check if user name exists
    }

    // Add user
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user); // Add user
        await _context.SaveChangesAsync(); // Save changes
    }
}