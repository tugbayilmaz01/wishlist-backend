using Microsoft.AspNetCore.Mvc;
using WishlistApi.Data;
using WishlistApi.Models;
using WishlistApi.Services;


namespace WishlistApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("Email already exists.");
            }


            user.PasswordHash = PasswordService.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            _context.SaveChanges();


            user.PasswordHash = null;

            return Ok(user);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            var existingUser = _context.Users.SingleOrDefault(u => u.Email == user.Email);
            if (existingUser == null) return BadRequest("User not found");

            if (!PasswordService.VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
                return BadRequest("Invalid password");

            return Ok(new { existingUser.Id, existingUser.Email });
        }


    }
}