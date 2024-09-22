using Xunit;
using Moq;
using TaskTracker.Controllers;
using TaskTracker.Data;
using TaskTracker.DTO;
using TaskTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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
    }
}