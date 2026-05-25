using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicBookingSystem.API.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        [Column(TypeName = "date")] // Chỉ lưu ngày YYYY-MM-DD
        public DateTime WorkDate { get; set; }

        [Required]
        [StringLength(50)]
        public string TimeSlot { get; set; } = string.Empty; // Ví dụ: "08:00 - 09:00"

        public bool IsAvailable { get; set; } = true; // Trống lịch hay đã bị đặt

        // Liên kết ngược về bảng Doctor
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;

        // Liên kết đến Lịch hẹn nếu khung giờ này được đặt
        public virtual Appointment? Appointment { get; set; }
    }
}
