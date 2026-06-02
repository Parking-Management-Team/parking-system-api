using Microsoft.EntityFrameworkCore;
using System.Reflection;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Data;

/// <summary>
/// Đối tượng Context kết nối cơ sở dữ liệu chính của dự án PBMS.
/// Quản lý vòng đời của các thực thể và thực hiện các giao dịch dữ liệu với PostgreSQL.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Khởi tạo một thực thể AppDbContext mới với các tùy chọn cấu hình được tiêm vào.
    /// </summary>
    /// <param name="options">Các tùy chọn cấu hình kết nối DB (Connection string, Provider,...).</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // =======================================================
    // KHU VỰC DÀNH CHO TEAM DEV:
    // Yêu cầu các thành viên thêm DbSet<T> của tính năng mình 
    // phụ trách vào bên dưới dòng này.
    // Ví dụ: public DbSet<Zone> Zones { get; set; }
    // =======================================================
    
    /// <summary>
    /// Tập hợp dữ liệu bảng Tài khoản (Accounts).
    /// </summary>
    public DbSet<Account> Accounts { get; set; }

    /// <summary>
    /// Tập hợp dữ liệu bảng Vai trò (Roles).
    /// </summary>
    public DbSet<Role> Roles { get; set; }

    // =======================================================

    /// <summary>
    /// Cấu hình các ánh xạ thực thể và mối quan hệ bảng thông qua Fluent API.
    /// </summary>
    /// <param name="modelBuilder">Đối tượng Builder thiết lập schema database.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Lệnh này giúp EF Core tự động quét và áp dụng các file 
        // cấu hình bảng (Fluent API) mà team dev tạo ra trong Assembly này
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}