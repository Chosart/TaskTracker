using Microsoft.AspNetCore.Mvc;
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
    public class TrackedTaskControllerTests
    {
        private readonly TrackedTaskController _controller;
        private readonly TaskTrackerContext _context;

        public TrackedTaskControllerTests()
        {
            // Використовуємо in-memory базу данних
            var options = new DbContextOptionsBuilder<TaskTrackerContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TaskTrackerContext(options);
            _controller = new TrackedTaskController(_context);
        }

        [Fact]
        public async Task GetTrackedTasks_ReturnsAllTasks()
        {
            // Створюємо трековану задачу
            var trackedTask = new TrackedTask
            {
                Title = "Test Task",
                Description = "Test Description",
                IsCompleted = false,
                Priority = "High",
                CreatedAt = 1234567890,
                UserId = 1
            };

            // Додаємо задачу в контекст
            _context.TrackedTasks.Add(trackedTask);
            await _context.SaveChangesAsync();

            // Ініціалізуємо контролер для трекованих задач
            var trackedTaskController = new TrackedTaskController(_context);
            var result = await trackedTaskController.GetTrackedTasks();

            // Перевірка, чи повертається список задач
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            // Перевіряємо, що в списку лише одна задача
            Assert.Single(tasks);
        }


    }
}
