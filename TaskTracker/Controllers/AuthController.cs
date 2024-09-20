using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Data;

namespace TaskTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TaskTrackerContext _context;
        private readonly string _tokenKey;

        public AuthController(TaskTrackerContext context, IConfiguration config)
        {
            _context = context;
            _tokenKey = config["AppSettings:Token"];
        }
    }
}
