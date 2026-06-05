using PBMS.API.Middlewares;
using PBMS.Application;
using PBMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CẤU HÌNH DỊCH VỤ (SERVICES CONTAINER)
// =========================================================================

// Đăng ký các Controller vào DI Container để ASP.NET Core nhận diện các API endpoints
builder.Services.AddControllers();

// Cấu hình OpenAPI (Swagger) phục vụ việc chạy tài liệu API
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Đăng ký các dịch vụ của tầng Application và Infrastructure
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

// Áp dụng định tuyến và ánh xạ các Controller
app.MapControllers();

app.Run();