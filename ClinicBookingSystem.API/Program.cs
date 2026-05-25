using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ClinicBookingSystem.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. ĐĂNG KÝ DỊCH VỤ CƠ SỞ DỮ LIỆU
builder.Services.AddDbContext<ClinicBookingSystem.API.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. CẤU HÌNH HỆ THỐNG BẢO MẬT XÁC THỰC JWT
var keyStr = "ChuoiChiaKhoaBiMatSieuCapBaoMatCuaPhongKham2026";
var key = Encoding.UTF8.GetBytes(keyStr);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = "ClinicBookingBackend",
        ValidateAudience = true,
        ValidAudience = "ClinicBookingFrontend",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();

// 3. ĐĂNG KÝ BUILT-IN OPENAPI MẶC ĐỊNH CỦA .NET 10 (THAY THẾ SWAGGER CŨ)
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Kích hoạt sinh file định nghĩa API 
    app.MapOpenApi();

    // Tận dụng công cụ giao diện UI để kết nối trực tiếp với file OpenAPI gốc của hệ thống
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Clinic Booking API v1");
    });
}

app.UseHttpsRedirection();

// Thứ tự bắt buộc: Đọc thẻ xong mới phân quyền vào cửa
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
