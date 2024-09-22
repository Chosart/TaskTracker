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

namespace TaskTracker.Tests
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly Mock<TaskTrackerContext> _mockContext;
        private readonly Mock<IConfiguration> _mockConfig;
        public AuthControllerTests()
        {
            // Створюємо Mock для TaskTrackerContext
            _mockContext = new Mock<TaskTrackerContext>();

            // Створюємо Mock для TaskTrackerContext
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["AppSettings:Token"]).Returns("TestToken");
            _mockConfig.Setup(c => c["AppSettings:Issuer"]).Returns("TestIssuer");
            _mockConfig.Setup(c => c["AppSettings:Audience"]).Returns("TestAudience");

            // Передаємо Mock об'єкти в контролер
            _controller = new AuthController(_mockContext.Object, _mockConfig.Object);
        }

        [Fact]
        public async Task Register_UserAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                UserName = "existingUser",
                Email = "existing@example.com",
                Password = "Passord123"
            };

            // Створюємо мок для Users
            var users = new List<User>()
            {
                new User { UserName = "existingUser"}
            }.AsQueryable();

            var mockSet = new Mock<DbSet<User>>();
            mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
            // Налаштування для перевірки наявності користувача
            _mockContext.Setup(c => c.Users.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .ReturnsAsync(true); // Симулюємо, що користувач існує

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("User already exists", result.ErrorMessage);
        }
    }
}