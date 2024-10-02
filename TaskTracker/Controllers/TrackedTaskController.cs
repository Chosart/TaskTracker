using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Data;
using TaskTracker.Models;
using Microsoft.EntityFrameworkCore;
using TaskTracker.DTO;
using System.Data;

namespace TaskTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackedTaskController : ControllerBase
    {
        private readonly TaskTrackerContext _context;

        public TrackedTaskController(TaskTrackerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrackedTask>>> GetTrackedTasks()
        {
            return await _context.TrackedTasks.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrackedTask>> GetTrackedTaskById(int id)
        {
            var trackedTask = await _context.TrackedTasks.FindAsync(id);
            if (trackedTask == null)
            {
                return NotFound();
            }
            return Ok(trackedTask);
        }

        // Метод для фільтрації задач
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<TrackedTask>>> FilterTrackedTasks([FromQuery] TaskFilterDto filter)
        {
            // Створюємо запит, який буде базуватися на таблиці TrackedTasks
            var query = _context.TrackedTasks.AsQueryable();

            // Додаємо фільтрацію за статусом, якщо він переданий
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(t => t.Status == filter.Status);
            }

            // Додаємо фільтрацію за пріорітетом, якщо він переданий
            if (!string.IsNullOrEmpty(filter.TaskPriority))
            {
                query = query.Where(t => t.TaskPriority == filter.TaskPriority);
            }

            // Додає фільтрацію за датою створення, якщо вона передана
            if(filter.CreatedAfter.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= ((DateTimeOffset)filter.CreatedAfter.Value).ToUnixTimeSeconds());
            }

            // Додаємо фільтрацію за датою створення до, якщо вона передана
            if (filter.CreatedBefore.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= ((DateTimeOffset)filter.CreatedBefore.Value).ToUnixTimeSeconds());
            }

            // Викоконуємо запит до бази даних і повертаємо список задач
            var tasks = await query.ToListAsync();

            // Повертаємо результати
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<TrackedTask>> CreateTrackedTask([FromBody] CreateTrackedTaskDto taskDto)
        {
            if (taskDto == null)
            {
                return BadRequest("Task cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trackedTask = new TrackedTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                IsCompleted = taskDto.IsCompleted,
                Priority = taskDto.Priority,
                CreatedAt = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                UserId = taskDto.UserId
            };

            await _context.TrackedTasks.AddAsync(trackedTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrackedTaskById), new { id = trackedTask.Id }, trackedTask);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTrackedTask(int id, TrackedTask trackedTask)
        {
            if (id != trackedTask.Id)
            {
                return BadRequest("ID in the URL does not match the task ID.");
            }

            var existingTask = await _context.TrackedTasks.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound("Task not found.");
            }

            existingTask.Title = trackedTask.Title;
            existingTask.Description = trackedTask.Description;
            existingTask.IsCompleted = trackedTask.IsCompleted;
            existingTask.Priority = trackedTask.Priority;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!trackedTaskExists(id))
                {
                    return NotFound("Task not found.");
                }
                throw; // Якщо існує помилка, залишаємо без змін
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTrackedTask(int id)
        {
            var trackedTask = await _context.TrackedTasks.FindAsync(id);
            if (trackedTask == null)
            {
                return NotFound("Task not found.");
            }

            _context.TrackedTasks.Remove(trackedTask);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }

            return NoContent();
        }

        private bool trackedTaskExists(int id)
        {
            return _context.TrackedTasks.Any(e => e.Id == id);
        }
    }
}
