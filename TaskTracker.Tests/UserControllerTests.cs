using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracker.Controllers;
using TaskTracker.Data;
using TaskTracker.Models;
using Xunit;

namespace TaskTracker.Tests
{
    public class UserControllerTests
    {
        private readonly UserController _controller;
        private readonly TaskTrackerContext _context;

        public UserControllerTests()
        {
            // Використовуємо in-memory базу данних
            var options = new DbContextOptionsBuilder<TaskTrackerContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _context = new TaskTrackerContext(options);
            _controller = new UserController(_context);

            // Додаємо тестових користувачів безпосередньо тут
            _context.Users.AddRange(new List<User>
            {
                new User { Id = 1, UserName = "User1", Email = "user1@example.com" },
                new User { Id = 2, UserName = "User2", Email = "user2@example.com" }
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetUsers_ReturnsListOfUsers()
        {
            // Act
            var result = await _controller.GetUsers();

            //Accert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
        }

        [Fact]
        public async Task GetUser_ExistingUser_ReturnsOkResult()
        {
            // Act
            var result = await _controller.GetUser(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<User>>(result);
            var returnedUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal("User1", returnedUser.UserName);
        }

        [Fact]
        public async Task GetUser_NonExistingUser_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetUser(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }


        [Fact]
        public async Task CreateUser_ValidUser_ReturnsCreatedResult()
        {
            // Arrange
            var newUser = new User
            {
                UserName = "User3",
                Email = "user3@gmail.com",
                PasswordHash = "Password123"
            };

            // Act
            var result = await _controller.CreateUser(newUser);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdUser = Assert.IsType<User>(createdResult.Value);
            Assert.Equal("User3", createdUser.UserName);
        }

        [Fact]
        public async Task CreateUser_NullUser_ReturnsBadRequest()
        {
            // Act
            var resut = await _controller.CreateUser(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resut.Result);
            Assert.Equal("User cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ExistingUser_ReturnsNoContent()
        {
            // Arrange
            var updatedUser = new User
            {
                Id = 1,
                UserName = "UpdatedUser1",
                Email = "updated1@gmail.com"
            };

            // Act
            var result = await _controller.UpdateUser(updatedUser.Id, updatedUser);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var user = await _context.Users.FindAsync(1);
            Assert.Equal("UpdatedUser1", user.UserName);
        }

        [Fact]
        public async Task UpdateUser_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var updateUser = new User
            {
                Id = 999,
                UserName = "NonExistentUser"
            };

            // Act
            var result = await _controller.UpdateUser(updateUser.Id, updateUser);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }
    }
}
