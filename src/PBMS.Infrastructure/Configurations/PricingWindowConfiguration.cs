using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng PricingWindow (Khung giờ giá) sử dụng Fluent API.
    /// </summary>
    public class PricingWindowConfiguration : IEntityTypeConfiguration<PricingWindow>
    {
        public void Configure(EntityTypeBuilder<PricingWindow> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("pricing_window");

            // 2. Khóa chính
            builder.HasKey(pw => pw.Id);

            // 3. Cột khóa chính
            builder.Property(pw => pw.Id)
                .HasColumnName("pricing_window_id")
                .ValueGeneratedOnAdd();

            // 4. Khóa ngoại PricingPolicyId
            builder.Property(pw => pw.PricingPolicyId)
                .HasColumnName("pricing_policy_id")
                .IsRequired();

            // 5. Tên khung giờ (WindowName)
            builder.Property(pw => pw.WindowName)
                .HasColumnName("window_name")
                .HasMaxLength(50)
                .IsRequired();

            // 6. Giờ bắt đầu (StartTime) - lưu kiểu time trong DB
            builder.Property(pw => pw.StartTime)
                .HasColumnName("start_time")
                .HasColumnType("time")
                .IsRequired();

            // 7. Giờ kết thúc (EndTime) - lưu kiểu time trong DB
            builder.Property(pw => pw.EndTime)
                .HasColumnName("end_time")
                .HasColumnType("time")
                .IsRequired();

            // 8. Thời lượng cơ bản (BaseDurationMinutes)
            builder.Property(pw => pw.BaseDurationMinutes)
                .HasColumnName("base_duration_minutes")
                .IsRequired();

            // 9. Giá cơ bản (BasePrice) - decimal(18,2)
            builder.Property(pw => pw.BasePrice)
                .HasColumnName("base_price")
                .HasPrecision(18, 2)
                .IsRequired();

            // 10. Kích thước block phát sinh (IncrementBlockMinutes)
            builder.Property(pw => pw.IncrementBlockMinutes)
                .HasColumnName("increment_block_minutes")
                .IsRequired();

            // 11. Giá mỗi block phát sinh (IncrementPrice) - decimal(18,2)
            builder.Property(pw => pw.IncrementPrice)
                .HasColumnName("increment_price")
                .HasPrecision(18, 2)
                .IsRequired();

            // 12. Mức giá tối đa của window (WindowCap) - decimal(18,2), cho phép null
            builder.Property(pw => pw.WindowCap)
                .HasColumnName("window_cap")
                .HasPrecision(18, 2);

            // 13. Thời gian ân hạn (GracePeriodMinutes)
            builder.Property(pw => pw.GracePeriodMinutes)
                .HasColumnName("grace_period_minutes")
                .HasDefaultValue(0)
                .IsRequired();

            // 14. Concurrency RowVersion
            builder.Property(pw => pw.RowVersion)
                .IsRowVersion();

            // 15. Thời điểm tạo
            builder.Property(pw => pw.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều khung giờ thuộc về 1 chính sách giá (PricingPolicy)
            // Khi xóa chính sách giá cha, tự động xóa các khung giờ con tương ứng (Cascade)
            builder.HasOne(pw => pw.PricingPolicy)
                .WithMany(pp => pp.PricingWindows)
                .HasForeignKey(pw => pw.PricingPolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
