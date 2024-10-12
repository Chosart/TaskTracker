using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Data;
using TaskTracker.Models;
using Microsoft.EntityFrameworkCore;
using TaskTracker.DTO;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TaskTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackedTaskController : ControllerBase
    {
        private readonly TaskTrackerContext _context;
        private readonly ILogger<TrackedTaskController> _logger;

        public TrackedTaskController(TaskTrackerContext context, ILogger<TrackedTaskController> logger)
        {
            _context = context;
            _logger = logger;
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
            var nullCheckResult = CheckForNull(filter, "Filter cannot be null.");
            if (nullCheckResult != null) return nullCheckResult;

            if (filter.CreatedAfter.HasValue && filter.CreatedBefore.HasValue && filter.CreatedAfter > filter.CreatedBefore)
            {
                return BadRequest("CreatedAfter must be earlier than CreatedBefore.");
            }

            if (string.IsNullOrEmpty(filter.Status) && filter.Statuses == null)
            {
                return BadRequest("At least one status must be provided.");
            }

            var query = _context.TrackedTasks.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(t => t.Status == filter.Status);
            }

            if (!string.IsNullOrEmpty(filter.Priority))
            {
                query = query.Where(t => t.Priority == filter.Priority);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(t => t.UserId == filter.UserId.Value);
            }

            if (filter.Statuses != null && filter.Statuses.Any())
            {
                query = query.Where(t => filter.Statuses.Contains(t.Status));
            }

            if (filter.CreatedAfter.HasValue)
            {
                var createdAfterUnix = ((DateTimeOffset)filter.CreatedAfter.Value).ToUnixTimeSeconds();
                query = query.Where(t => t.CreatedAt >= createdAfterUnix);
            }

            if (filter.CreatedBefore.HasValue)
            {
                var createdBeforeUnix = ((DateTimeOffset)filter.CreatedBefore.Value).ToUnixTimeSeconds();
                query = query.Where(t => t.CreatedAt <= createdBeforeUnix);
            }

            if (filter.Limit.HasValue)
            {
                query = query.Take(filter.Limit.Value);
            }

            var tasks = await query.ToListAsync();

            return tasks.Any() ? Ok(tasks) : new ActionResult<IEnumerable<TrackedTask>>(new List<TrackedTask>());
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

        private ActionResult CheckForNull(object obj, string errorMessage)
        {
            if (obj == null)
            {
                return BadRequest(errorMessage);
            }
            return null;
        }
    }
}
