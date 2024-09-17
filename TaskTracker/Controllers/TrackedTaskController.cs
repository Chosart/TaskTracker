using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Data;

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
    }
}
