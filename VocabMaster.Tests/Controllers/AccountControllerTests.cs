using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using AutoMapper;
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
        private readonly Mock<ITempDataDictionary> _tempData;

        public AccountControllerTests()
        {
            _mockAccountService = new Mock<IAccountService>();
            _mockMapper = new Mock<IMapper>();
            _tempData = new Mock<ITempDataDictionary>();

            _controller = new AccountController(_mockAccountService.Object, _mockMapper.Object)
            {
                TempData = _tempData.Object
            };
        }

        [Fact]
        public void Login_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Login();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Login_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new LoginRequestDto { Name = "", Password = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Login_Post_WithValidCredentials_RedirectsToHome()
        {
            // Arrange
            var model = new LoginRequestDto { Name = "testuser", Password = "password" };
            var user = new User { Id = 1, Name = "testuser" };
            _mockAccountService.Setup(svc => svc.Login(model.Name, model.Password)).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_Post_WithInvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginRequestDto { Name = "wronguser", Password = "wrongpass" };
            _mockAccountService.Setup(svc => svc.Login(model.Name, model.Password)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public void Register_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Register();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Register_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new RegisterRequestDto { Name = "", Password = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Register_Post_WithValidModel_RedirectsToLogin()
        {
            // Arrange
            var model = new RegisterRequestDto { Name = "newuser", Password = "password" };
            var user = new User { Name = "newuser", Password = "password" };
            
            _mockMapper.Setup(m => m.Map<User>(model)).Returns(user);
            _mockAccountService.Setup(svc => svc.Register(user)).ReturnsAsync(true);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            _mockAccountService.Verify(svc => svc.Register(user), Times.Once);
        }

        [Fact]
        public async Task Register_Post_WithExistingUsername_ReturnsViewWithError()
        {
            // Arrange
            var model = new RegisterRequestDto { Name = "existinguser", Password = "password" };
            var user = new User { Name = "existinguser", Password = "password" };
            
            _mockMapper.Setup(m => m.Map<User>(model)).Returns(user);
            _mockAccountService.Setup(svc => svc.Register(user)).ReturnsAsync(false);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task Logout_RedirectsToLogin()
        {
            // Arrange
            _mockAccountService.Setup(svc => svc.Logout()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            _mockAccountService.Verify(svc => svc.Logout(), Times.Once);
        }
    }
} 