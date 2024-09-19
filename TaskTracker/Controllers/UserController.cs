using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.Models;

namespace TaskTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly TaskTrackerContext _context;

        public UserController(TaskTrackerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new {id = user.Id}, user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<User>> UpdateUser (int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest("ID in the URL does not match the user ID.");
            }
           
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.PasswordHash = user.PasswordHash;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!UserExists(id))
                {
                    return NotFound("Task not found.");
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Task not found.");
            }

            _context.Users.Remove(user);

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

        private bool UserExists(int id)
        {
            return _context.Users.Any(u => u.Id == id);
        }
    }
}
