﻿using Microsoft.AspNetCore.Mvc;
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
    }
}
