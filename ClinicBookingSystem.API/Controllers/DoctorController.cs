using ClinicBookingSystem.API.Data;
using ClinicBookingSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")] // Khóa toàn bộ Controller này, chỉ cho tài khoản Bác sĩ vào cửa
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API ĐĂNG KÝ KHUNG GIỜ LÀM VIỆC CỦA BÁC SĨ (POST: api/Doctor/create-schedule)
        [HttpPost("create-schedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto model)
        {
            // 1. Kiểm tra xem Bác sĩ này có tồn tại trong hệ thống bảng Doctors chưa
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == model.DoctorId);
            if (doctor == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin Bác sĩ này trong hệ thống!" });
            }

            // 2. Ngăn chặn đăng ký lịch làm việc trong quá khứ
            if (model.WorkDate.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "Không thể đăng ký lịch làm việc cho các ngày trong quá khứ!" });
            }

            // 3. Kiểm tra chống trùng khung giờ làm việc đã đăng ký trước đó
            var isScheduleExist = await _context.Schedules.AnyAsync(s =>
                s.DoctorId == model.DoctorId &&
                s.WorkDate.Date == model.WorkDate.Date &&
                s.TimeSlot == model.TimeSlot);

            if (isScheduleExist)
            {
                return BadRequest(new { message = "Khung giờ này bạn đã đăng ký làm việc trước đó rồi!" });
            }

            // 4. Nếu mọi thứ hợp lệ, tiến hành lưu vào cơ sở dữ liệu
            var newSchedule = new Schedule
            {
                DoctorId = model.DoctorId,
                WorkDate = model.WorkDate.Date,
                TimeSlot = model.TimeSlot,
                IsAvailable = true // Mặc định khung giờ mới tạo sẽ ở trạng thái Trống lịch
            };

            _context.Schedules.Add(newSchedule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký khung giờ làm việc thành công!" });
        }
    }

    // Lớp trung gian hứng dữ liệu đăng ký lịch từ phía Client
    public class CreateScheduleDto
    {
        public int DoctorId { get; set; }
        public DateTime WorkDate { get; set; } // Ngày làm việc YYYY-MM-DD
        public string TimeSlot { get; set; } = string.Empty; // Ví dụ: "08:00 - 09:00"
    }
}
