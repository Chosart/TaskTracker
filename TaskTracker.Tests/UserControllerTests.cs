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
        public async Task GetUsers_ReturnsAllUsers()
        {
            // Створюємо користувача
            var user = new User
            {
                UserName = "Test",
                Email = "test@example.com"
            };

            // Додаємо користувача в контекст
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Ініціалізуємо контролер для користувачів
            var userController = new UserController(_context);
            var result = await userController.GetUsers();

            // Перевірка, чи повертається список користувачів
            var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var users = Assert.IsType<List<User>>(actionResult.Value);

            // Перевіряємо, що в списку лише один користувач
            Assert.Single(users);
        }
    }
}
