using Microsoft.EntityFrameworkCore;
using Moq;
using VocabMaster.Core.Entities;
using VocabMaster.Data;
using VocabMaster.Data.Repositories;

namespace VocabMaster.Tests.Repositories
{
    public class UserRepoTests
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly AppDbContext _context;
        private readonly UserRepo _userRepo;

        public UserRepoTests()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "VocabMaster_TestDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(_options);
            _userRepo = new UserRepo(_context);
        }

        [Fact]
        public async Task GetByName_ExistingUser_ReturnsUser()
        {
            // Arrange
            var user = new User { Name = "testuser", Password = "hashedpassword" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.GetByName("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Name);
        }

        [Fact]
        public async Task GetByName_NonExistingUser_ReturnsNull()
        {
            // Act
            var result = await _userRepo.GetByName("nonexistentuser");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetById_ExistingUser_ReturnsUserWithLearnedVocabularies()
        {
            // Arrange
            var user = new User { Name = "testuser", Password = "hashedpassword" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var learnedWord = new LearnedWord { Word = "test", UserId = user.Id };
            await _context.LearnedVocabularies.AddAsync(learnedWord);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.GetById(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Name);
            Assert.NotNull(result.LearnedVocabularies);
            Assert.Single(result.LearnedVocabularies);
            Assert.Equal("test", result.LearnedVocabularies.First().Word);
        }

        [Fact]
        public async Task GetById_NonExistingUser_ReturnsNull()
        {
            // Act
            var result = await _userRepo.GetById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task IsNameExist_ExistingName_ReturnsTrue()
        {
            // Arrange
            var user = new User { Name = "testuser", Password = "hashedpassword" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.IsNameExist("testuser");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsNameExist_NonExistingName_ReturnsFalse()
        {
            // Act
            var result = await _userRepo.IsNameExist("nonexistentuser");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Add_NewUser_AddsToDatabase()
        {
            // Arrange
            var user = new User { Name = "newuser", Password = "password" };

            // Act
            await _userRepo.Add(user);

            // Assert
            var addedUser = await _context.Users.FirstOrDefaultAsync(u => u.Name == "newuser");
            Assert.NotNull(addedUser);
            Assert.Equal("newuser", addedUser.Name);
            Assert.Equal("password", addedUser.Password);
        }

        [Fact]
        public async Task ValidateUser_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password");
            var user = new User { Name = "testuser", Password = hashedPassword };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.ValidateUser("testuser", "password");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Name);
        }

        [Fact]
        public async Task ValidateUser_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password");
            var user = new User { Name = "testuser", Password = hashedPassword };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepo.ValidateUser("testuser", "wrongpassword");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateUser_NonExistingUser_ReturnsNull()
        {
            // Act
            var result = await _userRepo.ValidateUser("nonexistentuser", "password");

            // Assert
            Assert.Null(result);
        }
    }
} 