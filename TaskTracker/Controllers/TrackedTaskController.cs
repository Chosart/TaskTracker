using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Data;
using TaskTracker.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
