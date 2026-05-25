using ClinicBookingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.API.Data
{
    // Lớp này kế thừa từ DbContext của Entity Framework Core để quản lý Database
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Định nghĩa các bảng dữ liệu sẽ được tạo trong SQL Server
        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình ngăn chặn trùng lịch khám ở tầng cơ sở dữ liệu (Unique Constraint)
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.ScheduleId)
                .IsUnique();

            // 1. Khi xóa một Bệnh nhân, không tự động xóa chùm Lịch hẹn
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict); // Thay đổi từ Cascade sang Restrict

            // 2. Khi xóa một Khung giờ làm việc, không tự động xóa chùm Lịch hẹn
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Schedule)
                .WithOne(s => s.Appointment)
                .HasForeignKey<Appointment>(a => a.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict); // Thay đổi từ Cascade sang Restrict
        }
    }
}
