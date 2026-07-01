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
    /// Tập hợp dữ liệu bảng Nhật ký thao tác (AuditLogs).
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Danh sách đen (Blacklists).
    /// </summary>
    public DbSet<Blacklist> Blacklists { get; set; } = null!;

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
    /// Tập hợp dữ liệu bảng Sự cố (Incidents).
    /// </summary>
    public DbSet<Incident> Incidents { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Thông báo (Notifications).
    /// </summary>
    public DbSet<Notification> Notifications { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Danh mục loại sự cố (IncidentTypes).
    /// </summary>
    public DbSet<IncidentType> IncidentTypes { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Phương tiện (Vehicles).
    /// </summary>
    public DbSet<Vehicle> Vehicles { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Đặt chỗ trước (Bookings).
    /// </summary>
    public DbSet<Booking> Bookings { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Đăng ký vé tháng (MonthlySubscriptions).
    /// </summary>
    public DbSet<MonthlySubscription> MonthlySubscriptions { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Giao dịch thanh toán (Payments).
    /// </summary>
    public DbSet<Payment> Payments { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Thống kê doanh thu (RevenueStatistics).
    /// </summary>
    public DbSet<RevenueStatistic> RevenueStatistics { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng trung gian đối soát doanh thu - thanh toán (RevenueStatisticPayments).
    /// </summary>
    public DbSet<RevenueStatisticPayment> RevenueStatisticPayments { get; set; } = null!;

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

    public DbSet<PricingRule> PricingRules { get; set; } = null!;
    public DbSet<BasePricingRuleConfig> BasePricingRuleConfigs { get; set; } = null!;
    public DbSet<IncrementPricingRuleConfig> IncrementPricingRuleConfigs { get; set; } = null!;
    public DbSet<DailyCapRuleConfig> DailyCapRuleConfigs { get; set; } = null!;
    public DbSet<GracePeriodRuleConfig> GracePeriodRuleConfigs { get; set; } = null!;
    public DbSet<PricingCalculationLog> PricingCalculationLogs { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Quyền hạn (Permissions).
    /// </summary>
    public DbSet<Permission> Permissions { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng trung gian Vai trò - Quyền hạn (RolePermissions).
    /// </summary>
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Cấu hình giá vé tháng (SubscriptionPriceConfigs).
    /// </summary>
    public DbSet<SubscriptionPriceConfig> SubscriptionPriceConfigs { get; set; } = null!;

    /// <summary>
    /// Tập hợp dữ liệu bảng Cấu hình giá phạt sự cố (PenaltyConfigs).
    /// </summary>
    public DbSet<PenaltyConfig> PenaltyConfigs { get; set; } = null!;
    
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

        // Cấu hình Value Converter để tự động chuyển DateTime sang UTC khi ghi/đọc DB, tránh lỗi timezone trên PostgreSQL/Supabase
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : null,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }

        // Áp dụng Global Query Filter cho tất cả các thực thể kế thừa ISoftDeletable
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var propertyMethodInfo = typeof(Microsoft.EntityFrameworkCore.EF).GetMethod("Property")!.MakeGenericMethod(typeof(bool));
                var isDeletedProperty = System.Linq.Expressions.Expression.Call(propertyMethodInfo, parameter, System.Linq.Expressions.Expression.Constant("IsDeleted"));
                var compareExpression = System.Linq.Expressions.Expression.MakeBinary(System.Linq.Expressions.ExpressionType.Equal, isDeletedProperty, System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
