using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Security.Cryptography;
using WishlistApi.Data;
using WishlistApi.Models;
using WishlistApi.Services;

namespace WishlistApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("AuthLimit")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public UsersController(AppDbContext context, JwtService jwtService, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
            _config = config;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("User ID not found in token");
            return userId;
        }

        
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDto userDto)
        {
            if (_context.Users.Any(u => u.Email == userDto.Email))
                return BadRequest(new { message = "Email already exists." });

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
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == userDto.Email);
            if (existingUser == null) return BadRequest(new { message = "Invalid email or password" });

            if (existingUser.PasswordHash == null || !PasswordService.VerifyPassword(userDto.Password, existingUser.PasswordHash))
                return BadRequest(new { message = "Invalid email or password" });

            var token = _jwtService.GenerateToken(existingUser.Id.ToString(), existingUser.Email);
            return Ok(new { existingUser.Id, existingUser.Email, Token = token });
        }


        [HttpPost("social-login")]
        public async Task<IActionResult> SocialLogin([FromBody] SocialLoginDto socialDto)
        {
            try
            {
                if (socialDto.Provider.ToLower() != "google")
                    return BadRequest(new { message = "Provider not supported" });

                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(socialDto.Token);

                if (jwtToken.Issuer != "accounts.google.com" && jwtToken.Issuer != "https://accounts.google.com")
                    return BadRequest(new { message = "Invalid token issuer" });

                var audience = jwtToken.Audiences.FirstOrDefault();
                if (audience != _jwtService.GetGoogleClientId())
                    return BadRequest(new { message = "Invalid token audience" });

                if (jwtToken.ValidTo < DateTime.UtcNow)
                    return BadRequest(new { message = "Token expired" });

                var payloadEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var payloadName = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

                if (string.IsNullOrEmpty(payloadEmail))
                    return BadRequest(new { message = "Email not found in token" });

                var existingUser = _context.Users.FirstOrDefault(u => u.Email == payloadEmail);

                if (existingUser == null)
                {
                    existingUser = new User
                    {
                        Email = payloadEmail,
                        Name = payloadName,
                        Avatar = null,
                        PasswordHash = null,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(existingUser);
                    await _context.SaveChangesAsync();
                }

                var token = _jwtService.GenerateToken(existingUser.Id.ToString(), existingUser.Email);
                return Ok(new { existingUser.Id, existingUser.Email, Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Social login failed: {ex.Message}" });
            }
        }

        
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe()
        {
            var userId = GetUserId();
            var user = _context.Users.Find(userId);
            if (user == null) return NotFound("User not found");

            return Ok(new { user.Id, user.Email, user.Name, user.Avatar });
        }

     
        [HttpPut("me")]
        [Authorize]
        public IActionResult UpdateMe([FromBody] UserUpdateDto userDto)
        {
            var userId = GetUserId();
            var user = _context.Users.Find(userId);
            if (user == null) return NotFound("User not found");

            user.Name = userDto.Name;
            user.Avatar = userDto.Avatar;
            _context.SaveChanges();

            return Ok(new { user.Id, user.Email, user.Name, user.Avatar });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

      
            if (user == null)
                return Ok(new { message = "If this email exists, a reset link has been sent." });

         
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL")
                              ?? _config["FrontendUrl"]
                              ?? "http://localhost:3000";
            var resetLink = $"{frontendUrl}/reset-password/{token}";

            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ForgotPassword] Email send failed: {ex.Message}");
                return StatusCode(500, new { message = "Failed to send reset email. Please try again later." });
            }

            return Ok(new { message = "If this email exists, a reset link has been sent." });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = _context.Users.SingleOrDefault(u => u.PasswordResetToken == dto.Token);

            if (user == null)
                return BadRequest(new { message = "Invalid or expired reset link." });

            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
                return BadRequest(new { message = "Reset link has expired. Please request a new one." });

            user.PasswordHash = PasswordService.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password updated successfully." });
        }
    }
}