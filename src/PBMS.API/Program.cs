using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PBMS.API.Middlewares;
using PBMS.Application;
using PBMS.Infrastructure;
using PBMS.Infrastructure.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CẤU HÌNH DỊCH VỤ (SERVICES CONTAINER)
// =========================================================================

// Đăng ký các Controller vào DI Container để ASP.NET Core nhận diện các API endpoints
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Cấu hình OpenAPI (Swagger) phục vụ việc chạy tài liệu API
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Đăng ký các dịch vụ của tầng Application và Infrastructure
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Cấu hình CORS (Cross-Origin Resource Sharing)
// Cho phép Web Frontend (chạy trên localhost:3000 hoặc localhost:5173) gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevelopment", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Cấu hình dịch vụ xác thực (Authentication) sử dụng JWT Bearer
var secretKey = builder.Configuration["Jwt:Key"] ?? "A_Super_Secret_Key_For_JWT_Auth_System_PBMS_Project_2026_SWP391";
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Tắt yêu cầu HTTPS khi chạy môi trường local (development)
    options.SaveToken = true;             // Lưu token lại trong HttpContext
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PBMS",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PBMSUsers",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero         // Loại bỏ thời gian trễ mặc định 5 phút của server
    };
});

var app = builder.Build();

// Tự động chạy Migration khi ứng dụng khởi động ở môi trường Development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
            Console.WriteLine("--> Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Error applying database migrations: {ex.Message}");
        }
    }
}

// =========================================================================
// 2. CẤU HÌNH PIPELINE XỬ LÝ REQUEST (MIDDLEWARES)
// =========================================================================

// Middleware xử lý lỗi toàn cục (Global Exception Handling)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Kích hoạt Swagger UI làm giao diện test API trực quan
app.UseSwagger();
app.UseSwaggerUI();

// Áp dụng cấu hình CORS đã định nghĩa phía trên
app.UseCors("AllowDevelopment");

// Kích hoạt Authentication (Xác thực người dùng) và Authorization (Phân quyền truy cập)
// Lưu ý: UseAuthentication PHẢI chạy trước UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

// Áp dụng định tuyến và ánh xạ các Controller
app.MapControllers();

app.Run();