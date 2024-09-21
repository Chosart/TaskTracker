using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly IConfiguration _configuration;

        public AuthController(TaskTrackerContext context, IConfiguration config)
        {
            _context = context;
            _tokenKey = config["AppSettings:Token"];
            _configuration = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
                return BadRequest(new { message = "User data is required." });

            if (await _context.Users.AnyAsync(u => u.UserName == registerDto.UserName))
                return BadRequest(new { message = "UserName is already taken." });

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return BadRequest(new { message = "Email is already in use." });

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email
            };

            user.SetPassword(registerDto.Password);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Internal server error while saving user." });
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest(new { message = "User data is required." });
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == loginDto.UserName);

            if (user == null || !user.ValidatePassword(loginDto.Password))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var token = GenerateJwtToken(user);

            return Ok(new TokenDto { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenExpirationString = _configuration["AppSettings:TokenExpiration"];
            if (!int.TryParse(tokenExpirationString, out int tokenExpiration))
            {
                tokenExpiration = 30;
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(tokenExpiration),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
