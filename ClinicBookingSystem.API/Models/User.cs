using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace ClinicBookingSystem.API.Models
{
    public class User
    {
        [Key] // Đánh dấu đây là Khóa chính
        public int Id { get; set; }

        [Required] // Bắt buộc phải nhập (Not Null)
        [StringLength(100)] // Giới hạn tối đa 100 ký tự (nvarchar(100))
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // 'Admin', 'Doctor', 'Patient'

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Mối quan hệ điều hướng (Navigation Properties)
        public virtual Doctor? Doctor { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
