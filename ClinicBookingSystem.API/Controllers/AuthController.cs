using ClinicBookingSystem.API.Data;
using ClinicBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ClinicBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API ĐĂNG KÝ TÀI KHOẢN (POST: api/Auth/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var isEmailExist = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (isEmailExist)
            {
                return BadRequest(new { message = "Email này đã được sử dụng!" });
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var newUser = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = passwordHash,
                Role = model.Role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }
    }

    public class RegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";
    }
}
