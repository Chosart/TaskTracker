using Xunit;
using Moq;
using TaskTracker.Controllers;
using TaskTracker.Data;
using TaskTracker.DTO;
using TaskTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TaskTracker.Tests
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly TaskTrackerContext _context;
        private readonly Mock<IConfiguration> _mockConfig;
        public AuthControllerTests()
        {
            // Використовуємо in-memory базу данних
            var options = new DbContextOptionsBuilder<TaskTrackerContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TaskTrackerContext(options);

            // Замокуємо конфігурації
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["AppSettings:Token"]).Returns("TestToken");
            _mockConfig.Setup(c => c["AppSettings:Issuer"]).Returns("TestIssuer");
            _mockConfig.Setup(c => c["AppSettings:Audience"]).Returns("TestAudience");

            // Передаємо контекст в контролер
            _controller = new AuthController(_context, _mockConfig.Object);
        }

        [Fact]
        public async Task Register_UserAlreadyExists_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto
            {
                UserName = "existingUser",
                Email = "existing@example.com",
                Password = "Password123"
            };

            // Додаємо існуючого користувача в базу даних
            _context.Users.Add(new User { UserName = registerDto.UserName, Email = registerDto.Email });
            await _context.SaveChangesAsync();

            var result = await _controller.Register(registerDto);

            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var errorResponse = actionResult.Value as dynamic;

            Assert.NotNull(errorResponse);
            Assert.Equal("UserName is already taken.", errorResponse.message);
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            var loginDto = new LoginDto
            {
                UserName = "nonExistentUser",
                Password = "Password123"
            };

            var result = await _controller.Login(loginDto);

            var actionresult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var errorResponse = actionresult.Value as dynamic;

            Assert.NotNull(errorResponse);
            Assert.Equal("Invalid username or password.", errorResponse.message);
        }
    }
}