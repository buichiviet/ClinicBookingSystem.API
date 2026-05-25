using ClinicBookingSystem.API.Data;
using ClinicBookingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

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

        // 1. API ĐĂNG KÝ TÀI KHOẢN (POST: api/Auth/register)
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

        // 2. API ĐĂNG NHẬP SINH JWT TOKEN (POST: api/Auth/login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác!" });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác!" });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var keyStr = "ChuoiChiaKhoaBiMatSieuCapBaoMatCuaPhongKham2026";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "ClinicBookingBackend",
                audience: "ClinicBookingFrontend",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                user = new { user.Id, user.FullName, user.Email, user.Role }
            });
        }

        // =========================================================================
        // 3. API KIỂM THỬ BẢO MẬT PHÂN QUYỀN 
        // =========================================================================
        [HttpGet("test-auth")]
        [Microsoft.AspNetCore.Authorization.Authorize] // Khóa bảo mật: Phải có Token mới gọi được
        public IActionResult TestAuth()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "Chúc mừng! Thẻ Token của bạn hoàn toàn hợp lệ để vượt qua hàng rào bảo mật.",
                extractedData = new { userId, userEmail, userRole }
            });
        }
    }

    public class RegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
