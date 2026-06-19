using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng RevenueStatistic (Thống kê doanh thu) sử dụng Fluent API.
    /// </summary>
    public class RevenueStatisticConfiguration : IEntityTypeConfiguration<RevenueStatistic>
    {
        public void Configure(EntityTypeBuilder<RevenueStatistic> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("revenue_statistic");

            // 2. Khóa chính
            builder.HasKey(rs => rs.Id);

            // 3. Cột khóa chính -> "statistic_id"
            builder.Property(rs => rs.Id)
                .HasColumnName("statistic_id")
                .ValueGeneratedOnAdd();

            // 4. Khóa ngoại liên kết tới Building
            builder.Property(rs => rs.BuildingId)
                .HasColumnName("building_id")
                .IsRequired();

            // 5. Khóa ngoại liên kết tới VehicleType
            builder.Property(rs => rs.VehicleTypeId)
                .HasColumnName("vehicle_type_id")
                .IsRequired(false);

            // 6. Ngày bắt đầu chu kỳ (StartDate) - column type: date
            builder.Property(rs => rs.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("date")
                .IsRequired();

            // 6. Ngày kết thúc chu kỳ (EndDate) - column type: date
            builder.Property(rs => rs.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("date")
                .IsRequired();

            // 7. Loại chu kỳ (PeriodType) - tối đa 20 ký tự
            builder.Property(rs => rs.PeriodType)
                .HasColumnName("period_type")
                .HasMaxLength(20)
                .IsRequired();

            // 8. Doanh thu (TotalRevenue) - decimal(18,2), mặc định 0
            builder.Property(rs => rs.TotalRevenue)
                .HasColumnName("total_revenue")
                .HasPrecision(18, 2)
                .HasDefaultValue(0.00m)
                .IsRequired();

            // 9. Tổng số đặt chỗ (TotalBookings) - mặc định 0
            builder.Property(rs => rs.TotalBookings)
                .HasColumnName("total_bookings")
                .HasDefaultValue(0)
                .IsRequired();

            // 10. Tổng số lượt gửi (TotalSessions) - mặc định 0
            builder.Property(rs => rs.TotalSessions)
                .HasColumnName("total_sessions")
                .HasDefaultValue(0)
                .IsRequired();

            // 11. Tổng số vé tháng (TotalSubscriptions) - mặc định 0
            builder.Property(rs => rs.TotalSubscriptions)
                .HasColumnName("total_subscriptions")
                .HasDefaultValue(0)
                .IsRequired();

            // 12. Concurrency RowVersion
            builder.Property(rs => rs.RowVersion)
                .IsRowVersion();

            // 13. Thời điểm tạo thống kê
            builder.Property(rs => rs.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều dòng thống kê có thể áp dụng cho cùng 1 Building
            builder.HasOne(rs => rs.Building)
                .WithMany(b => b.RevenueStatistics)
                .HasForeignKey(rs => rs.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều dòng thống kê có thể thuộc về cùng một loại xe
            builder.HasOne(rs => rs.VehicleType)
                .WithMany()
                .HasForeignKey(rs => rs.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
