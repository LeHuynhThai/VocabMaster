using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Entities;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetByNameAsync(string name)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
    }

    public async Task<bool> IsNameExistsAsync(string name)
    {
        return await _context.Users.AnyAsync(u => u.Name == name);
    }

    public async Task AddAsync(User user)
    {
        user.Role = UserRole.User;
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User> LoginAsync(string name, string password)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Name == name && u.Password == password);
    }
}