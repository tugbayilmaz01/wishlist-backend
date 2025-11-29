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
        private readonly JwtService _jwtService;

        public UsersController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDto userDto)
        {
            if (_context.Users.Any(u => u.Email == userDto.Email))
            {
                return BadRequest("Email already exists.");
            }

            var user = new User
            {
                Email = userDto.Email,
                PasswordHash = PasswordService.HashPassword(userDto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { user.Id, user.Email, user.CreatedAt });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto userDto)
        {
            var existingUser = _context.Users.SingleOrDefault(u => u.Email == userDto.Email);
            if (existingUser == null) return BadRequest("User not found");

            if (!PasswordService.VerifyPassword(userDto.Password, existingUser.PasswordHash))
                return BadRequest("Invalid password");

            var token = _jwtService.GenerateToken(existingUser.Id.ToString(), existingUser.Email);

            return Ok(new { existingUser.Id, existingUser.Email, Token = token });
        }


    }
}