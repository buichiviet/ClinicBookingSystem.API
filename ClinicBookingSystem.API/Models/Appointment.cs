using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicBookingSystem.API.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; } // ID người bệnh (Role = Patient)

        [Required]
        public int ScheduleId { get; set; } // ID khung giờ khám cụ thể

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid

        public string? Notes { get; set; } // Triệu chứng bệnh

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Thiết lập mối quan hệ liên kết dữ liệu
        [ForeignKey("PatientId")]
        public virtual User Patient { get; set; } = null!;

        [ForeignKey("ScheduleId")]
        public virtual Schedule Schedule { get; set; } = null!;
    }
}
