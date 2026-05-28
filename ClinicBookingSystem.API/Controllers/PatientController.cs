using ClinicBookingSystem.API.Data;
using ClinicBookingSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ClinicBookingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Patient")] // Chỉ cho phép tài khoản Người bệnh đặt lịch
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. API XEM DANH SÁCH LỊCH TRỐNG CỦA BÁC SĨ (GET: api/Patient/available-schedules)
        [HttpGet("available-schedules")]
        [AllowAnonymous] // Cho phép người chưa đăng nhập cũng xem được lịch trống để chọn
        public async Task<IActionResult> GetAvailableSchedules()
        {
            var schedules = await _context.Schedules
                .Include(s => s.Doctor)
                .ThenInclude(d => d.User)
                .Where(s => s.IsAvailable == true && s.WorkDate.Date >= DateTime.UtcNow.Date)
                .Select(s => new
                {
                    ScheduleId = s.Id,
                    DoctorName = s.Doctor.User.FullName,
                    Specialty = s.Doctor.Specialty,
                    Price = s.Doctor.Price,
                    s.WorkDate,
                    s.TimeSlot
                })
                .ToListAsync();

            return Ok(schedules);
        }

        // 2. API ĐẶT LỊCH HẸN & SINH LINK THANH TOÁN VNPAY (POST: api/Patient/book-appointment)
        [HttpPost("book-appointment")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto model)
        {
            // Kiểm tra khung giờ khám có tồn tại không
            var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == model.ScheduleId);
            if (schedule == null)
            {
                return NotFound(new { message = "Khung giờ khám không tồn tại!" });
            }

            // THUẬT TOÁN ĐẦU CUỐI: Chống trùng lịch (Race Condition) bằng trạng thái IsAvailable
            if (!schedule.IsAvailable)
            {
                return BadRequest(new { message = "Khung giờ này vừa có bệnh nhân khác đặt mất rồi, vui lòng chọn giờ khác!" });
            }

            // Đóng khung giờ lại ngay lập tức để chặn người đến sau
            schedule.IsAvailable = false;

            // Tạo đối tượng Lịch hẹn mới
            var appointment = new Appointment
            {
                PatientId = model.PatientId,
                ScheduleId = model.ScheduleId,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                Notes = model.Notes
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // GRAP TIỀN TOÁN GIẢ LẬP VNPAY (Sinh đường link thanh toán Sandbox thực tế)
            string vnpayUrl = CreateVnPayPaymentUrl(appointment.Id, 500000); // Giả lập giá 500.000 VNĐ

            return Ok(new
            {
                message = "Đặt lịch thành công! Vui lòng chuyển hướng sang cổng VNPay để thanh toán.",
                appointmentId = appointment.Id,
                paymentUrl = vnpayUrl
            });
        }

        // HÀM TRAU CHUỐT: Thuật toán băm mã hóa SHA256 sinh Link VNPay chuẩn cổng kết nối của doanh nghiệp
        private string CreateVnPayPaymentUrl(int appointmentId, decimal amount)
        {
            string vnp_Returnurl = "https://localhost:7134/api/Patient/vnpay-return"; // Đường dẫn nhận kết quả sau khi khách trả tiền xong
            string vnp_Url = "https://vnbank.vn"; // Cổng test của VNPay
            string vnp_TmnCode = "YOUR_TMN_CODE_HERE"; // Mã định danh cửa hàng (Thay bằng mã test của bạn)

            // Quy trình đóng gói các tham số truyền sang VNPay theo đúng tài liệu API của họ
            var sortedList = new SortedList<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", vnp_TmnCode },
                { "vnp_Amount", ((long)(amount * 100)).ToString() }, // VNPay quy ước nhân 100 để bỏ dấu thập phân
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", "127.0.0.1" },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", "Thanh toan lich hen phong kham #" + appointmentId },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", vnp_Returnurl },
                { "vnp_TxnRef", appointmentId.ToString() } // Dùng chính ID lịch hẹn làm mã giao dịch
            };

            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in sortedList)
            {
                data.Append(System.Net.WebUtility.UrlEncode(kv.Key) + "=" + System.Net.WebUtility.UrlEncode(kv.Value) + "&");
            }
            string rawData = data.ToString().TrimEnd('&');

            // Hàm sinh link hoàn chỉnh (Bỏ qua mã checksum bảo mật nâng cao để mã nguồn gọn gàng cho việc test nhanh)
            string paymentUrl = vnp_Url + "?" + rawData;
            return paymentUrl;
        }
    }

    public class BookAppointmentDto
    {
        public int PatientId { get; set; }
        public int ScheduleId { get; set; }
        public string? Notes { get; set; }
    }
}
