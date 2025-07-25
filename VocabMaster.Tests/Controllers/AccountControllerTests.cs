using Microsoft.AspNetCore.Mvc;
using Moq;
using AutoMapper;
using System.Text.Json;
using VocabMaster.API.Controllers;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _mockAccountService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockAccountService = new Mock<IAccountService>();
            _mockMapper = new Mock<IMapper>();

            _controller = new AccountController(_mockAccountService.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Login_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new LoginRequestDto { Name = "", Password = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Login(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            // Arrange
            var model = new LoginRequestDto { Name = "testuser", Password = "password" };
            var user = new User { Id = 1, Name = "testuser", Role = UserRole.User };
            _mockAccountService.Setup(svc => svc.Login(model.Name, model.Password)).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseJson = JsonSerializer.Serialize(okResult.Value);
            var responseObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseJson);

            Assert.True(responseObj.ContainsKey("id"));
            Assert.Equal(1, responseObj["id"].GetInt32());
            
            Assert.True(responseObj.ContainsKey("name"));
            Assert.Equal("testuser", responseObj["name"].GetString());
            
            Assert.True(responseObj.ContainsKey("role"));
            Assert.Equal("User", responseObj["role"].GetString());
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var model = new LoginRequestDto { Name = "wronguser", Password = "wrongpass" };
            _mockAccountService.Setup(svc => svc.Login(model.Name, model.Password)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(model);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Register_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new RegisterRequestDto { Name = "", Password = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Register(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_WithValidModel_ReturnsOk()
        {
            // Arrange
            var model = new RegisterRequestDto { Name = "newuser", Password = "password" };
            var user = new User { Name = "newuser", Password = "password" };
            
            _mockMapper.Setup(m => m.Map<User>(model)).Returns(user);
            _mockAccountService.Setup(svc => svc.Register(user)).ReturnsAsync(true);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseJson = JsonSerializer.Serialize(okResult.Value);
            var responseObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseJson);
            
            Assert.True(responseObj.ContainsKey("success"));
            Assert.True(responseObj["success"].GetBoolean());
            
            Assert.True(responseObj.ContainsKey("message"));
            Assert.Equal("Registration successful", responseObj["message"].GetString());
        }

        [Fact]
        public async Task Register_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var model = new RegisterRequestDto { Name = "existinguser", Password = "password" };
            var user = new User { Name = "existinguser", Password = "password" };
            
            _mockMapper.Setup(m => m.Map<User>(model)).Returns(user);
            _mockAccountService.Setup(svc => svc.Register(user)).ReturnsAsync(false);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var responseJson = JsonSerializer.Serialize(badRequestResult.Value);
            var responseObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseJson);
            
            Assert.True(responseObj.ContainsKey("message"));
            Assert.Equal("Username already exists", responseObj["message"].GetString());
        }

        [Fact]
        public async Task Logout_ReturnsOk()
        {
            // Arrange
            _mockAccountService.Setup(svc => svc.Logout()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseJson = JsonSerializer.Serialize(okResult.Value);
            var responseObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseJson);
            
            Assert.True(responseObj.ContainsKey("success"));
            Assert.True(responseObj["success"].GetBoolean());
        }
        
        [Fact]
        public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsOk()
        {
            // Arrange
            var user = new User { 
                Id = 1, 
                Name = "testuser", 
                Role = UserRole.User,
                LearnedVocabularies = new List<LearnedWord> { new LearnedWord() }
            };
            _mockAccountService.Setup(svc => svc.GetCurrentUser()).ReturnsAsync(user);

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseJson = JsonSerializer.Serialize(okResult.Value);
            var responseObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseJson);
            
            Assert.True(responseObj.ContainsKey("id"));
            Assert.Equal(1, responseObj["id"].GetInt32());
            
            Assert.True(responseObj.ContainsKey("name"));
            Assert.Equal("testuser", responseObj["name"].GetString());
            
            Assert.True(responseObj.ContainsKey("role"));
            Assert.Equal("User", responseObj["role"].GetString());
            
            Assert.True(responseObj.ContainsKey("learnedWordsCount"));
            Assert.Equal(1, responseObj["learnedWordsCount"].GetInt32());
        }
        
        [Fact]
        public async Task GetCurrentUser_WithUnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange
            _mockAccountService.Setup(svc => svc.GetCurrentUser()).ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
} 