using Microsoft.EntityFrameworkCore;
using PBMS.Domain.Entities;
using System.Reflection;

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
    public DbSet<Account> Accounts { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Vai trò (Roles).
    /// </summary>
    public DbSet<Role> Roles { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Thẻ gửi xe (Cards).
    /// Mỗi Card là một mã thẻ mô phỏng dùng để nhận diện lượt gửi xe.
    /// </summary>
    public DbSet<Card> Cards { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Khu vực (Zones).
    /// </summary>
    public DbSet<Zone> Zones { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Tòa nhà (Buildings).
    /// </summary>
    public DbSet<Building> Buildings { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Tầng (Floors).
    /// </summary>
    public DbSet<Floor> Floors { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Vị trí đỗ xe (ParkingSlots).
    /// </summary>
    public DbSet<ParkingSlot> ParkingSlots { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Lượt gửi xe (ParkingSessions).
    /// </summary>
    public DbSet<ParkingSession> ParkingSessions { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Phương tiện (Vehicles).
    /// </summary>
    public DbSet<Vehicle> Vehicles { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Loại phương tiện (VehicleTypes).
    /// </summary>
    public DbSet<VehicleType> VehicleTypes { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Chính sách giá (PricingPolicies).
    /// </summary>
    public DbSet<PricingPolicy> PricingPolicies { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Khung giờ giá (PricingWindows).
    /// </summary>
    public DbSet<PricingWindow> PricingWindows { get; set; } = null!;
    

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