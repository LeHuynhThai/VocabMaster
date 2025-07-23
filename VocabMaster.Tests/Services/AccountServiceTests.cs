using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AutoMapper;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Services;

namespace VocabMaster.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IUserRepo> _mockUserRepo;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AccountService _accountService;
        private readonly Mock<HttpContext> _mockHttpContext;

        public AccountServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepo>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockMapper = new Mock<IMapper>();
            _mockHttpContext = new Mock<HttpContext>();
            
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
            
            _accountService = new AccountService(
                _mockUserRepo.Object,
                _mockHttpContextAccessor.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task Register_WithNewUsername_ReturnsTrue()
        {
            // Arrange
            var user = new User { Name = "testuser", Password = "password" };
            _mockUserRepo.Setup(repo => repo.IsNameExist(user.Name)).ReturnsAsync(false);
            _mockUserRepo.Setup(repo => repo.Add(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _accountService.Register(user);

            // Assert
            Assert.True(result);
            _mockUserRepo.Verify(repo => repo.Add(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ReturnsFalse()
        {
            // Arrange
            var user = new User { Name = "existinguser", Password = "password" };
            _mockUserRepo.Setup(repo => repo.IsNameExist(user.Name)).ReturnsAsync(true);

            // Act
            var result = await _accountService.Register(user);

            // Assert
            Assert.False(result);
            _mockUserRepo.Verify(repo => repo.Add(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var user = new User { Id = 1, Name = "testuser", Password = "hashedpassword", Role = UserRole.User };
            _mockUserRepo.Setup(repo => repo.ValidateUser("testuser", "password")).ReturnsAsync(user);
            
            // Setup mock authentication
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(auth => auth.SignInAsync(
                    It.IsAny<HttpContext>(),
                    It.IsAny<string>(),
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            _mockHttpContext.Setup(ctx => ctx.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = await _accountService.Login("testuser", "password");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Name, result.Name);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsNull()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.ValidateUser("wronguser", "wrongpassword")).ReturnsAsync((User)null);

            // Act
            var result = await _accountService.Login("wronguser", "wrongpassword");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrentUser_WhenAuthenticated_ReturnsUser()
        {
            // Arrange
            var user = new User { Id = 1, Name = "testuser", Role = UserRole.User };
            
            // Tạo mock cho ClaimsPrincipal
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            var userIdClaim = new Claim("UserId", "1");
            
            // Setup FindFirst để trả về claim UserId
            mockClaimsPrincipal.Setup(cp => cp.FindFirst("UserId")).Returns(userIdClaim);
            mockClaimsPrincipal.Setup(cp => cp.Identity.IsAuthenticated).Returns(true);
            
            _mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);
            _mockUserRepo.Setup(repo => repo.GetById(1)).ReturnsAsync(user);

            // Act
            var result = await _accountService.GetCurrentUser();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Name, result.Name);
        }

        [Fact]
        public async Task GetCurrentUser_WhenNotAuthenticated_ReturnsNull()
        {
            // Arrange
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            mockClaimsPrincipal.Setup(cp => cp.Identity.IsAuthenticated).Returns(false);
            _mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);

            // Act
            var result = await _accountService.GetCurrentUser();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrentUser_WithInvalidUserId_ReturnsNull()
        {
            // Arrange
            // Create mock for ClaimsPrincipal
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            var userIdClaim = new Claim("UserId", "invalid");
            
            // Setup FindFirst to return invalid UserId claim
            mockClaimsPrincipal.Setup(cp => cp.FindFirst("UserId")).Returns(userIdClaim);
            mockClaimsPrincipal.Setup(cp => cp.Identity.IsAuthenticated).Returns(true);
            
            _mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);

            // Act
            var result = await _accountService.GetCurrentUser();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrentUser_WithNoUserIdClaim_ReturnsNull()
        {
            // Arrange
            // Create mock for ClaimsPrincipal
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            
            // Setup FindFirst to return null (no UserId claim)
            mockClaimsPrincipal.Setup(cp => cp.FindFirst("UserId")).Returns((Claim)null);
            mockClaimsPrincipal.Setup(cp => cp.Identity.IsAuthenticated).Returns(true);
            
            _mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);

            // Act
            var result = await _accountService.GetCurrentUser();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void HashPassword_ReturnsNonEmptyString()
        {
            // Arrange
            var password = "password";

            // Act
            var result = _accountService.HashPassword(password);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.NotEqual(password, result);
        }
    }
} 