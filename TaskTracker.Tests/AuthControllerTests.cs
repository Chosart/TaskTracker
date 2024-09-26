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
            // ������������� in-memory ���� ������
            var options = new DbContextOptionsBuilder<TaskTrackerContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TaskTrackerContext(options);

            // �������� ������������
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["AppSettings:Token"]).Returns("TestToken");
            _mockConfig.Setup(c => c["AppSettings:Issuer"]).Returns("TestIssuer");
            _mockConfig.Setup(c => c["AppSettings:Audience"]).Returns("TestAudience");

            // �������� �������� � ���������
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

            var existingUser = new User
            {
                UserName = "existingUser",
                Email = "existing@example.com"
            };

            // ������ ��������� ����������� � ���� �����
            existingUser.SetPassword("Password123"); // ������ ������
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var result = await _controller.Register(registerDto);

            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var errorResponse = Assert.IsType<ErrorResponse>(actionResult.Value);

            Assert.NotNull(errorResponse);
            // �������� ����������� �����������
            Assert.Equal("UserName is already taken.", errorResponse.Message);
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            var loginDto = new LoginDto
            {
                UserName = "nonExistentUser",
                Password = "SomePassword"
            };

            var result = await _controller.Login(loginDto);

            var actionResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var errorResponse = Assert.IsType<ErrorResponse>(actionResult.Value);

            Assert.NotNull(errorResponse);
            Assert.Equal("Invalid username or password.", errorResponse.Message);
        }

        [Fact]
        public async Task UpdateTrackedTask_ValidTask_ReturnOk()
        {
            // ��������� ��������� ������
            var trackedTask = new TrackedTask
            {
                Title = "Original Title",
                Description = "Original Description",
                IsCompleted = false,
                Priority = "High",
                CreatedAt = 1234567890,
                UserId = 1
            };

            // ������ ������ � ��������
            _context.TrackedTasks.Add(trackedTask);
            await _context.SaveChangesAsync();

            var updateTaskDto = new TrackedTaskDto
            {
                Title = "Update Title",
                Description = "Update Description",
                IsCompleted = true,
                Priority = "Low"
            };

            // ���������� ���������
            var trackedTaskController = new TrackedTaskController(_context);

            // ��������� ����� ���������
            var result = await trackedTaskController.UpdateTrackedTask(trackedTask.Id, updateTaskDto);

            // ����������, �� ��������� - �� OkResult
            var actionResult = Assert.IsType<ActionResult<TrackedTask>>(result);
            var updateTask = Assert.IsType<TrackedTask>(actionResult.Value);

            // ����������, �� ������� ����
            Assert.Equal(updateTaskDto.Title, updateTask.Title);
            Assert.Equal(updateTaskDto.Description, updateTask.Description);
            Assert.True(updateTask.IsCompleted);
            Assert.Equal(updateTaskDto.Priority, updateTask.Priority);
        }

        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            // ��������� �����������
            var user = new User
            {
                UserName = "Test",
                Email = "test@example.com"
            };

            // ������ ����������� � ��������
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ���������� ��������� ��� ������������
            var userController = new UserController(_context);
            var result = await userController.GetUsers();

            // ��������, �� ����������� ������ ������������
            var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var users = Assert.IsType<List<User>>(actionResult.Value);

            // ����������, �� � ������ ���� ���� ����������
            Assert.Single(users);
        }   
    }
}