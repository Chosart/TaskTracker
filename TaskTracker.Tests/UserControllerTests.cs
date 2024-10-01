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

            // Очищення бази даних перед тестами
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }
        private void SeedDatabase()
        {
            if (!_context.Users.Any())
            {
                _context.Users.AddRange(new List<User>
                {
                    new User
                    {
                        Id = 1,
                        UserName = "User1",
                        Email = "user1@example.com",
                        PasswordHash = "hashedpassword",
                        Salt = "somesalt"
                    },
                    new User
                    {
                        Id = 2,
                        UserName = "User2",
                        Email = "user2@example.com",
                        PasswordHash = "hashedpassword",
                        Salt = "somesalt"
                    }
                });

                _context.SaveChanges();
            }
        }

        [Fact]
        public async Task GetUsers_ReturnsListOfUsers()
        {
            // Arrange
            SeedDatabase();

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
            Assert.Equal(2, users.Count());
        }

        [Fact]
        public async Task GetUser_ExistingUser_ReturnsOkResult()
        {
            // Act
            var result = await _controller.GetUser(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<User>(okResult.Value);

            Assert.NotNull(returnedUser);
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
            var user = new User
            {
                UserName = "User3",
                Email = "user3@gmail.com",
                PasswordHash = "hashedpassword",
                Salt = "somesalt"
            };

            // Act
            var result = await _controller.CreateUser(user);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdUser = Assert.IsType<User>(createdResult.Value);
            Assert.Equal(user.UserName, createdUser.UserName);
        }

        [Fact]
        public async Task CreateUser_NullUser_ReturnsBadRequest()
        {
            // Arrange
            User nullUser = null;

            // Act
            var result = await _controller.CreateUser(nullUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("User cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ExistingUser_ReturnsNoContent()
        {
            // Arrange
            var existingUser = await _context.Users.FindAsync(1);
            existingUser.UserName = "UpdatedUser1";
            existingUser.Email = "updated1@gmail.com";

            // Act
            var result = await _controller.UpdateUser(existingUser.Id, existingUser);

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

        [Fact]
        public async Task DeleteUser_ExistingUser_ReturnsNoContent()
        {
            // Arrange
            SeedDatabase();

            // Act
            var result = await _controller.DeleteUser(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.Users.FindAsync(1)); // Перевіряємо, що користувача більше немає
        }

        [Fact]
        public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteUser(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }
    }
}
