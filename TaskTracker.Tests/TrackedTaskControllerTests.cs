﻿using Microsoft.AspNetCore.Mvc;
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

        [Fact]
        public async Task UpdateTrackedTask_ValidTask_ReturnOk()
        {
            // Створюємо трековану задачу
            var trackedTask = new TrackedTask
            {
                Title = "Original Title",
                Description = "Original Description",
                IsCompleted = false,
                Priority = "High",
                CreatedAt = 1234567890,
                UserId = 1
            };

            // Додаємо задачу в контекст
            _context.TrackedTasks.Add(trackedTask);
            await _context.SaveChangesAsync();

            var updateTaskDto = new TrackedTaskDto
            {
                Title = "Update Title",
                Description = "Update Description",
                IsCompleted = true,
                Priority = "Low"
            };

            // Ініціалізуємо контролер
            var trackedTaskController = new TrackedTaskController(_context);

            // Викликаємо метод оновлення
            var result = await trackedTaskController.UpdateTrackedTask(trackedTask.Id, updateTaskDto);

            // Перевіряємо, що результат - це OkResult
            var actionResult = Assert.IsType<ActionResult<TrackedTask>>(result);
            var updateTask = Assert.IsType<TrackedTask>(actionResult.Value);

            // Перевіряємо, чи оновлені данні
            Assert.Equal(updateTaskDto.Title, updateTask.Title);
            Assert.Equal(updateTaskDto.Description, updateTask.Description);
            Assert.True(updateTask.IsCompleted);
            Assert.Equal(updateTaskDto.Priority, updateTask.Priority);
        }
    }
}
