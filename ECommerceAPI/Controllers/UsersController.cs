using ECommerceAPI.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, IConfiguration configuration, ILogger<UsersController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            _logger.LogInformation("Registering a new user with email: {Email}", user.Email);
            var userExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (userExists)
            {
                _logger.LogWarning("Registration failed: User already exists with email: {Email}", user.Email);
                return BadRequest("User already exists.");
            }

            var newUser = new User
            {
                Email = user.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash),
                FullName = user.FullName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User registered successfully with email: {Email}", user.Email);

            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {Id}", id);
                return NotFound();
            }
            return user;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(User login)
        {
            _logger.LogInformation("Attempting login for user with email: {Email}", login.Email);
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (dbUser == null)
            {
                _logger.LogWarning("Login failed: User not found with email: {Email}", login.Email);
                return Unauthorized("User not found.");
            }

            if (string.IsNullOrEmpty(dbUser.PasswordHash) || !BCrypt.Net.BCrypt.Verify(login.PasswordHash, dbUser.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid credentials for user with email: {Email}", login.Email);
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(dbUser.Id);
            _logger.LogInformation("User logged in successfully with email: {Email}", login.Email);
            return Ok(new
            {
                Token = token,
                Email = dbUser.Email,
                FullName = dbUser.FullName,
                Role = dbUser.Role
            });
        }

        private string GenerateJwtToken(int userId)
        {
            var secret = _configuration["JwtConfig:Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
