using Microsoft.EntityFrameworkCore;
using VocabMaster.Core.Entities;

namespace VocabMaster.Data.Repositories
{
    // Repository thao tác với dữ liệu người dùng (User)
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _context;

        // Hàm khởi tạo repository, inject context
        public UserRepo(AppDbContext context)
        {
            _context = context;
        }

        // Lấy user theo tên
        public async Task<User> GetByName(string name)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
        }

        // Lấy user theo Id (bao gồm cả danh sách từ đã học)
        public async Task<User> GetById(int id)
        {
            return await _context.Users
                .Include(u => u.LearnedVocabularies)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // Kiểm tra tên user đã tồn tại chưa
        public async Task<bool> IsNameExist(string name)
        {
            return await _context.Users.AnyAsync(u => u.Name == name);
        }

        // Thêm mới user
        public async Task Add(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        // Xác thực user (kiểm tra tên và mật khẩu)
        public async Task<User> ValidateUser(string name, string password)
        {
            // Lấy user theo tên
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == name);

            // Nếu không tìm thấy user hoặc mật khẩu không đúng
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }

            return user;
        }
    }
}