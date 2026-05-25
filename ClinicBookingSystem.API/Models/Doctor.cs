using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicBookingSystem.API.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialty { get; set; } = string.Empty; // Chuyên khoa

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Định dạng tiền tệ chuẩn SQL Server
        public decimal Price { get; set; } // Giá khám

        public string? Biography { get; set; } // Tiểu sử bác sĩ (có thể trống)

        // Quan hệ liên kết ngược về bảng Users
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Danh sách các khung giờ làm việc của bác sĩ này
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
