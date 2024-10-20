﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Controllers;
using TaskTracker.Data;
using TaskTracker.DTO;
using TaskTracker.Models;

namespace TaskTracker.Tests
{
    public class TrackedTaskControllerTests
    {
        private readonly TrackedTaskController _controller;
        private readonly TaskTrackerContext _context;
        private readonly Mock<ILogger<TrackedTaskController>> _mockLogger;

        public TrackedTaskControllerTests()
        {
            // Використовуємо in-memory базу данних
            var options = new DbContextOptionsBuilder<TaskTrackerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new TaskTrackerContext(options);
            _mockLogger = new Mock<ILogger<TrackedTaskController>>();
            _controller = new TrackedTaskController(_context, _mockLogger.Object);

            // Очищення бази данних перед кожним тестом
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
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
                Status = "Open",
                CreatedAt = 1234567890,
                UserId = 1
            };

            // Додаємо задачу в контекст
            _context.TrackedTasks.Add(trackedTask);
            await _context.SaveChangesAsync();

            // Виклик методу
            var result = await _controller.GetTrackedTasks();

            // Перевірка, чи повертається список задач
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            // Перевіряємо, що в списку лише одна задача
            Assert.Single(tasks);
        }

        [Fact]
        public async Task UpdateTrackedTask_ValidTask_ReturnOk()
        {
            // Створюємо трековану задачу
            var trackedTask = new TrackedTask
            {
                Id = 1,
                Title = "Original Title",
                Status = "Open",
                Description = "Original Description",
                IsCompleted = false,
                Priority = "High",
                CreatedAt = 1234567890,
                UserId = 1
            };

            // Додаємо задачу в контекст
            _context.TrackedTasks.Add(trackedTask);
            await _context.SaveChangesAsync();

            // Створюємо об'єкт для оновлення з новими даними
            var updatedTask = new TrackedTask
            {
                Id = trackedTask.Id, // Вказуємо правильний ID
                Title = "Update Title",
                Status = "Open",
                Description = "Update Description",
                IsCompleted = true, // Ставимо true для IsCompleted
                Priority = "Low",
                CreatedAt = trackedTask.CreatedAt, // Залишаємо ту ж саму дату створення
                UserId = trackedTask.UserId // Залишаємо того ж користувача
            };

            // Викликаємо метод оновлення
            var result = await _controller.UpdateTrackedTask(trackedTask.Id, updatedTask);

            // Перевіряємо, що результат - це NoContentResult
            var actionResult = Assert.IsType<NoContentResult>(result);

            // Перевіряємо, чи оновлені дані
            var updatedEntity = await _context.TrackedTasks.FindAsync(trackedTask.Id);
            Assert.NotNull(updatedEntity); // Переконуємось, що задача існує
            Assert.Equal(updatedTask.Title, updatedEntity.Title);
            Assert.Equal(updatedTask.Description, updatedEntity.Description);
            Assert.True(updatedEntity.IsCompleted); // Переконуємось, що IsCompleted стало true
            Assert.Equal(updatedTask.Priority, updatedEntity.Priority);
        }

        [Fact]
        public async Task CreateTrackedTask_ValidTask_ReturnsCreatedTask()
        {
            // Підготовка данних для створення нової задачі
            var createTaskDto = new CreateTrackedTaskDto
            {
                Title = "New Task",
                Description = "New Description",
                IsCompleted = false,
                Priority = "Medium",
                Status = "Open",
                UserId = 1
            };

            // Виклик методу створення задачі
            var result = await _controller.CreateTrackedTask(createTaskDto);

            // Перевірка, що результат - це CreatedAtActionResult
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);

            // Перевірка, що створена задача має правильні значення 
            var createdTask = Assert.IsType<TrackedTask>(actionResult.Value);

            Assert.Equal(createTaskDto.Title, createdTask.Title);
            Assert.Equal(createTaskDto.Description, createdTask.Description);
            Assert.False(createdTask.IsCompleted);
            Assert.Equal(createTaskDto.Priority, createdTask.Priority);
            Assert.Equal(createTaskDto.Status, createdTask.Status);
        }

        [Fact]
        public async Task DeleteTrackedTask_ExistingId_ReturnsNoContent()
        {
            // Створення трекової задачі для видалення
            var trackedTask = new TrackedTask
            {
                Title = "Task to Delete",
                Status = "Closed",
                Description = "Description",
                IsCompleted = false,
                Priority = "Low",
                CreatedAt = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                UserId = 1
            };

            // Створюємо задачу в контекст
            _context.TrackedTasks.Add(trackedTask);
            await _context.SaveChangesAsync();
             
            // Виклик методу видалення задачі
            var result = await _controller.DeleteTrackedTask(trackedTask.Id);

            // Перевірка, що результат - це NoContentResult
            Assert.IsType<NoContentResult>(result);

            // Перевірка, що задача була видалена з бази
            Assert.Null(await _context.TrackedTasks.FindAsync(trackedTask.Id));
        }

        [Fact]
        public async Task DeleteTrackedTask_NonExistingId_ReturnsNotFound()
        {
            // Виклик методу видалення для неіснуючого ID
            var result = await _controller.DeleteTrackedTask(999); // Невірний ID

            // Перевірка, що результат - це NotFoundObjectResult
            var actionResult = Assert.IsType<NotFoundObjectResult>(result);

            // Перевірка повідомлення про помилку
            Assert.Equal("Task not found.", actionResult.Value);
        }
    }
}
