using Microsoft.AspNetCore.Http.HttpResults;
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

        [Fact]
        public async Task UpdateTaskPriority_ExistingTask_ReturnsNoContent()
        {
            // Arrange
            SeedTasks();
            var task = await _context.TrackedTasks.FirstAsync(); // Отримуємо існуючу задачу
            task.Status = "Closed"; // Змінюємо статус на Closed

            // Act
            var result = await _controller.UpdateTrackedTask(task.Id, task);    

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedEntity = await _context.TrackedTasks.FindAsync(task.Id);
            Assert.Equal("Closed", updatedEntity.Status);
        }

        [Fact]
        public async Task FilterTrackedTasks_ByUser_ReturnsFilteredTasks()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { UserId = 1 }; // Фільтрація за UserId
            var result =  await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Equal(3, tasks.Count);
        }

        [Fact]
        public async Task FilterTrackedTasks_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { Status = null };
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task FilterTrackedTasks_MultipleFilters_ReturnsCorrectTasks()
        {
            // Arrange
            SeedTasks();
            var filterDto = new TaskFilterDto { Status = "Open", TaskPriority = "High" };
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Single(tasks);
            Assert.Equal("Task 1", tasks[0].Title);
        }

        [Fact]
        public async Task FilterTrackedTasks_MultipleStatuses_ReturnsFilteredTasks()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { Statuses = new List<string> { "Open", "Closed" } }; // Додайте поле для кількох статусів
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Equal(3, tasks.Count); // Має бути 3 задачі з різними статусами
        }

        [Fact]
        public async Task FilterTrackedTasks_NoFilters_ReturnsAllTasks()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto(); // Порожній фільтр
            var result = await _controller.FilterTrackedTasks(filterDto);

            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Equal(3, tasks.Count); //  Має повернути всі задачі
        }

        [Fact]
        public async Task FilterTrackedTask_LimitResults_ReturnsLimitedTasks()
        {
            // Arrange 
            SeedTasks();

            var filterDto = new TaskFilterDto { Limit = 2 };
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Equal(2, tasks.Count);
        }

        [Fact]
        public async Task FilterTrackedTasks_NoMatchingPriority_ReturnsEmptyList()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { TaskPriority = "NonExistentPriority" };
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);   
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Empty(tasks);
        }

        [Fact]
        public async Task FilterTrackedTasks_ByCreatedBetween_ReturnsFilteredTasks()
        {
            // Arrange
            SeedTasks();

            var fromDate = DateTime.UtcNow.AddDays(-3);
            var toDate = DateTime.UtcNow.AddDays(-1);
            var filterDto = new TaskFilterDto { CreatedAfter = fromDate, CreatedBefore = toDate };

            // Act
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Empty(tasks);
            Assert.Equal("Task 3", tasks[0].Title);
        }

        [Fact]
        public async Task FilterTrackedTasks_ByCreatedAfter_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange 
            SeedTasks();

            var filterDto = new TaskFilterDto { CreatedAfter = DateTime.UtcNow }; // Дата в майбутньому
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Empty(tasks);
        }

        [Fact]
        public async Task FilterTrackedTasks_InvalidData_AllFields_ReturnsBadRequest()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto
            {
                Status = null,
                TaskPriority = null,
                UserId = null,
                CreatedAfter = DateTime.MinValue,
                CreatedBefore = DateTime.MaxValue,
            };

            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            Assert.IsType<BadRequest>(result.Result);
        }

        [Fact]
        public async Task FilterTrackedTasks_ByNonExistentUser_ReturnsEmptyList()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { UserId = 999 }; // Користувач, якого немає
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value);

            Assert.Empty(tasks);  // Не має повертати жодну задачу
        }
    }
}
