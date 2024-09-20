using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.DTO;
using TaskTracker.Models;

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

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
                return BadRequest("User data is required.");

            // Перевірка на унікальність
            if (await _context.Users.AnyAsync(u => u.UserName == registerDto.UserName))
                return BadRequest("UserName is already taken.");

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return BadRequest("Email is already in use.");

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email
            };

            // Хешування пароля
            user.SetPassword(registerDto.Password);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal server error while saving user.");
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
    }
}
