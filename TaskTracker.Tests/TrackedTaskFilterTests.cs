﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Controllers;
using TaskTracker.Data;
using TaskTracker.DTO;
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

        [Fact]
        public async Task FilterTrackedTasks_ByStatus_ReturnsFilteredTasks()
        {
            // Arrange
            SeedTasks(); // Сіюємо задачі перед тестуванням

            var filterDto = new TaskFilterDto { Status = "Open" };
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Equal(2, tasks.Count); // Має бути 2 відкриті задачі
            Assert.All(tasks, task => Assert.Equal("Open", task.Status));
        }

        [Fact]
        public async Task FilterTrackedTasks_ByPriority_ReturnsFilteredTasks()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { TaskPriority = "High" };
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Single(tasks);
            Assert.Equal("Task 1", tasks[0].Title);
        }

        [Fact]
        public async Task FilterTrackedTasks_ByCreatedAfter_ReturnsFilteredTasks()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { CreatedAfter = DateTime.UtcNow.AddDays(-2) };
            var result = _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Equal(2, tasks.Count); // Має бути 2 задачі, створені після 2 днів тому
        }

        [Fact]
        public async Task FilterTrackedTasks_ByCreatedBefore_ReturnsFilteredTasks()
        {
            // Arrange
            SeedTasks();

            // Створюємо дату, до якої будемо фільтрувати
            var filterDto = new TaskFilterDto { CreatedBefore = DateTime.UtcNow.AddDays(-1) };
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Equal(2, tasks.Count); // Має бути 2 задачі, створені до 1 дня тому
            Assert.Contains(tasks, task => task.Title == "Task 1");
            Assert.Contains(tasks, task => task.Title == "Task 3");
        }

        [Fact]
        public async Task FilterTrackedTasks_ByStatusAndPriority_ReturnsFilteredTasks()
        {
            // Arrange 
            SeedTasks();

            var filterDto = new TaskFilterDto { Status = "Open", TaskPriority = "High" };
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Single(tasks); // Має бути 1 задача
            Assert.Equal("Task 1", tasks[0].Title); // Перевіряємо, що це правильна задача
        }

        [Fact]
        public async Task FilterTrackedTasks_NoMatchingStatus_ReturnsEmptyList()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { Status = "NoExistentStatus" };
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert 
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Empty(tasks);
        }
    }
}