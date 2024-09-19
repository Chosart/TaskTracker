using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Data;

namespace TaskTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskTrackerContext _context;

        public TasksController(TaskTrackerContext context)
        {
            _context = context;
        }

        [HttpPost("update-created-at")]
        public async Task<ActionResult> UpdateTasks()
        {
            await _context.UpdateCreatedAtValues();
            return Ok("CreatedAt values updated successfully.");
        }
    }
}
