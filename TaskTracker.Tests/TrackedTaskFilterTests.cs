using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Controllers;
using TaskTracker.Data;
using TaskTracker.Models;

namespace TaskTracker.Tests
{
    public class TrackedTaskFilterTests
    {
        private readonly TrackedTaskController _controller;
        private readonly TaskTrackerContext _context;

        public TrackedTaskFilterTests()
        {
            // Налаштування in-memory бази даних
            var options = new DbContextOptionsBuilder<TaskTrackerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new TaskTrackerContext(options);
            _controller = new TrackedTaskController(_context);

            // Очищення бази даних перед тестами
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        private void SeedTasks()
        {
            var task1 = new TrackedTask
            {
                Title = "Task 1",
                Status = "Open",
                Priority = "High",
                CreatedAt = (int)(DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds()), // 2 дні тому
                UserId = 1
            };

            var task2 = new TrackedTask
            {
                Title = "Task 2",
                Status = "Open",
                Priority = "Low",
                CreatedAt = (int)(DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds()), // 1 дні тому
                UserId = 1
            };

            var task3 = new TrackedTask
            {
                Title = "Task 2",
                Status = "Closed",
                Priority = "Medium",
                CreatedAt = (int)(DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds()), // 3 дні тому
                UserId = 1
            };

            _context.TrackedTasks.AddRange(task1, task2, task3);
            _context.SaveChanges();
        }
    }
}
