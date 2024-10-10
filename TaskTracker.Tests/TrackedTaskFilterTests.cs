using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
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
                .EnableSensitiveDataLogging()
                .Options;

            _context = new TaskTrackerContext(options);

            // Створення мокового логгера
            var loggerMock = new Mock<ILogger<TrackedTaskController>>();

            _controller = new TrackedTaskController(_context, loggerMock.Object);

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
                Description = "Description for Task 1",
                CreatedAt = (int)(DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds()), // 2 дні тому
                UserId = 1
            };

            var task2 = new TrackedTask
            {
                Title = "Task 2",
                Status = "Open",
                Priority = "Medium",
                Description = "Description for Task 2",
                CreatedAt = (int)(DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds()), // 1 дні тому
                UserId = 1
            };

            var task3 = new TrackedTask
            {
                Title = "Task 3",
                Status = "Closed",
                Priority = "Low",
                Description = "Description for Task 3",
                CreatedAt = (int)(DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds()), // 3 дні тому
                UserId = 1
            };

            _context.TrackedTasks.AddRange(task1, task2, task3);
            _context.SaveChanges();
        }

        [Theory]
        [InlineData("Open", 1)]
        [InlineData("In Progress", 1)]
        [InlineData("Closed", 1)]
        [InlineData("Nonexistent", 0)]
        public void FilterTrackedTasks_ByStatus_ReturnsFilteredTasks(string status, int expectedCount)
        {
            // Arrange
            var tasks = new List<TrackedTask>
            {
                new TrackedTask { Status = "Open" },
                new TrackedTask { Status = "In Progress" },
                new TrackedTask { Status = "Closed" }
            };

            var filterDto = new TaskFilterDto();

            // Act
            var result = filterDto.FilterTrackedTasks(tasks, null, status); // Передаємо null для priority

            // Assert
            Assert.Equal(expectedCount, result.Count);

            if (expectedCount > 0)
            {
                Assert.All(result, task => Assert.Equal(status, task.Status)); // Перевірка статусу
            }
        }

        [Theory]
        [InlineData(1, "High")]
        [InlineData(2, "Medium")]
        [InlineData(3, "Low")]
        public void FilterTrackedTasks_ByPriority_ReturnsCorrectTasks(int priority, string expectedPriority)
        {
            // Arrange
            var tasks = new List<TrackedTask>
            {
                new TrackedTask { Priority = "High", Title = "Task 1" },
                new TrackedTask { Priority = "Medium", Title = "Task 2" },
                new TrackedTask { Priority = "Low", Title = "Task 3" }
            };

            var filterDto = new TaskFilterDto();

            // Act
            var result = filterDto.FilterTrackedTasks(tasks, priority, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedPriority, result.First().Priority);
        }

        [Fact]
        public async Task FilterTrackedTasks_ByCreatedAfter_ReturnsFilteredTasks()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { CreatedAfter = DateTime.UtcNow.AddDays(-2) };
            var result = await _controller.FilterTrackedTasks(filterDto);

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

            Assert.NotNull(tasks);
            Assert.Equal(2, tasks.Count); // Має бути 2 задачі, створені до 1 дня тому
            Assert.Contains(tasks, task => task.Title == "Task 1");
            Assert.Contains(tasks, task => task.Title == "Task 3");
        }

        [Fact]
        public async Task FilterTrackedTasks_ByStatusAndPriority_ReturnsFilteredTasks()
        {
            // Arrange 
            SeedTasks();

            var filterDto = new TaskFilterDto { Status = "Open", Priority = "High" };
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

            var filterDto = new TaskFilterDto { Status = "NonExistingStatus" };     

            // Act
            var result = await _controller.FilterTrackedTasks(filterDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TrackedTask>>>(result);
            var tasks = Assert.IsType<List<TrackedTask>>(actionResult.Value); // Перевіряємо, що Value не null

            Assert.Empty(tasks); // Перевіряємо, що список задач пустий
        }

        [Fact]
        public async Task UpdateTaskPriority_ExistingTask_ReturnsNoContent()
        {
            // Arrange
            SeedTasks();

            var task = await _context.TrackedTasks.FirstAsync(); // Отримуємо існуючу задачу

            // Оновлюємо лише ті поля, які потрібні
            task.Status = "Closed"; // Змінюємо статус на Closed
            task.Priority = "Medium";

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
            var result = await _controller.FilterTrackedTasks(filterDto);

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
            var BadRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Filter cannot be null.", BadRequestResult.Value);
        }

        [Theory]
        [InlineData("Open", 1)]
        [InlineData("In Progress", 1)]
        [InlineData("Closed", 1)]
        [InlineData("Nonexistent", 0)]
        public async Task FilterTrackedTasks_MultipleFilters_ReturnsCorrectTasks(string status, int expectedCount) 
        {
            // Arrange
            var tasks = new List<TrackedTask>
            {
                new TrackedTask { Status = "Open", Priority = "High", Title = "Task 1", Description = "Description 1" },
                new TrackedTask { Status = "In Progress", Priority = "Medium", Title = "Task 2", Description = "Description 2" },
                new TrackedTask { Status = "Closed", Priority = "Low", Title = "Task 3", Description = "Description 3" }
            };

            _context.TrackedTasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filterDto = new TaskFilterDto
            {
                Statuses = new List<string> { "Open", "In Progress", "Closed" } // Вказуємо кілька статусів
            };

            // Act
            var filteredTasks = filterDto.FilterTrackedTasks(tasks, null, status);

            // Assert
            Assert.Equal(expectedCount, filteredTasks.Count);

            if (expectedCount > 0)
            {
                Assert.All(filteredTasks, task => Assert.Equal(status, task.Status));
            }
        }

        [Theory]
        [InlineData("Open", 1)]
        [InlineData("In Progress", 1)]
        [InlineData("Closed", 1)]
        [InlineData("Nonexistent", 0)]
        public void FilterTrackedTasks_MultipleStatuses_ReturnsFilteredTasks(string status, int expectedCount)
        {
            // Arrange
            var tasks = new List<TrackedTask>
            {
                new TrackedTask { Status = "Open", Priority = "High", Title = "Task 1" },
                new TrackedTask { Status = "In Progress", Priority = "Medium", Title = "Task 2" },
                new TrackedTask { Status = "Closed", Priority = "Low", Title = "Task 3" }
            };

            var filterDto = new TaskFilterDto
            {
                Statuses = new List<string> { "Open", "In Progress", "Closed" } // Вказуємо кілька статусів
            };

            // Act
            var filteredTasks = filterDto.FilterTrackedTasks(tasks, null, status);

            // Assert
            Assert.Equal(expectedCount, filteredTasks.Count); // Перевіряємо, що кількість задач відповідає очікуваній

            if (expectedCount > 0)
            {
                Assert.All(filteredTasks, task => Assert.Equal(status, task.Status)); // Перевіряємо, що статуси збігаються
            }
        }

        [Theory]
        [InlineData("Nonexistent", 0)]
        public async Task FilterTrackedTasks_NoFilters_ReturnsAllTasks(string status, int expectedCount)
        {
            // Arrange
            var tasks = new List<TrackedTask>
            {
                new TrackedTask { Status = "Open", Priority = "High", Title = "Task 1", Description = "Description 1" },
                new TrackedTask { Status = "In Progress", Priority = "Medium", Title = "Task 2", Description = "Description 2" },
                new TrackedTask { Status = "Closed", Priority = "Low", Title = "Task 3", Description = "Description 3" }
            };

            _context.TrackedTasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filterDto = new TaskFilterDto
            {
                Statuses = new List<string> { status } // Вказуємо кілька статусів
            };

            // Act
            var filteredTasks = filterDto.FilterTrackedTasks(tasks, null, status);

            // Assert
            Assert.Equal(expectedCount, filteredTasks.Count); // Перевіряємо, що кількість задач відповідає очікуваній

            if (expectedCount > 0)
            {
                Assert.All(filteredTasks, task => Assert.Equal(status, task.Status)); // Перевіряємо, що статуси збігаються
            }
        }

        [Theory]
        [InlineData(2)] // Тестуємо з лімітом 2
        public async Task FilterTrackedTasks_LimitResults_ReturnsLimitedTasks(int limit)
        {
            // Arrange
            var tasks = new List<TrackedTask>
            {
                new TrackedTask { Status = "Open", Priority = "High", Title = "Task 1", Description = "Description 1" },
                new TrackedTask { Status = "In Progress", Priority = "Medium", Title = "Task 2", Description = "Description 2" },
                new TrackedTask { Status = "Closed", Priority = "Low", Title = "Task 3", Description = "Description 3" }
            };

            _context.TrackedTasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filterDto = new TaskFilterDto { Limit = limit };

            // Act
            var filteredTasks = filterDto.FilterTrackedTasks(tasks, null, null); // Передаємо null для status

            // Assert
            Assert.NotNull(filteredTasks); // Перевіряємо, що результат не null
            Assert.Equal(limit, filteredTasks.Count); // Перевіряємо, що кількість задач відповідає ліміту
        }

        [Fact]
        public async Task FilterTrackedTasks_NoMatchingPriority_ReturnsEmptyList()
        {
            // Arrange
            SeedTasks();

            var filterDto = new TaskFilterDto { Priority = "NonExistentPriority" };
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

            Assert.Equal(3, tasks.Count); // Має повернути 3 задачі
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
            var filter = new TaskFilterDto
            {
                CreatedAfter = new DateTime(9999, 12, 31)
            };

            // Act
            var result = await _controller.FilterTrackedTasks(filter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("CreatedAfter date is out of range.", badRequestResult.Value);
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
