using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng Payment (Giao dịch thanh toán) sử dụng Fluent API.
    /// </summary>
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // 1. Ánh xạ bảng vật lý và cấu hình ràng buộc kiểm tra CHECK
            builder.ToTable("payment", t => t.HasCheckConstraint(
                "CK_Payment_Source",
                "(CASE WHEN session_id IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN booking_id IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN monthly_subscription_id IS NOT NULL THEN 1 ELSE 0 END) = 1"
            ));

            // 2. Khóa chính
            builder.HasKey(p => p.Id);

            // 3. Cột khóa chính -> "payment_id"
            builder.Property(p => p.Id)
                .HasColumnName("payment_id")
                .ValueGeneratedOnAdd();

            // 4. Các khóa ngoại (cho phép null vì giao dịch chỉ thuộc 1 trong 3 nguồn nghiệp vụ)
            builder.Property(p => p.SessionId)
                .HasColumnName("session_id");

            builder.Property(p => p.BookingId)
                .HasColumnName("booking_id");

            builder.Property(p => p.MonthlySubscriptionId)
                .HasColumnName("monthly_subscription_id");

            builder.Property(p => p.PricingPolicyId)
                .HasColumnName("pricing_policy_id");

            // 5. Số tiền (Amount) - decimal(18,2)
            builder.Property(p => p.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();

            // 6. Phương thức thanh toán (PaymentMethod) - tối đa 20 ký tự
            builder.Property(p => p.PaymentMethod)
                .HasColumnName("payment_method")
                .HasMaxLength(20)
                .IsRequired();

            // 7. Thời điểm thanh toán thực tế (PaymentTime)
            builder.Property(p => p.PaymentTime)
                .HasColumnName("payment_time");

            // 8. Trạng thái giao dịch (PaymentStatus) - tối đa 20 ký tự
            builder.Property(p => p.PaymentStatus)
                .HasColumnName("payment_status")
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .IsRequired();

            // 9. Concurrency RowVersion
            builder.Property(p => p.RowVersion)
                .IsRowVersion();

            // 10. Thời điểm tạo giao dịch
            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều giao dịch có thể thuộc về 1 lượt gửi xe (ParkingSession)
            builder.HasOne(p => p.Session)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều giao dịch có thể thuộc về 1 lượt đặt chỗ (Booking)
            builder.HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều giao dịch có thể thuộc về 1 hồ sơ vé tháng (MonthlySubscription)
            builder.HasOne(p => p.MonthlySubscription)
                .WithMany(ms => ms.Payments)
                .HasForeignKey(p => p.MonthlySubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều giao dịch có thể lưu vết 1 chính sách giá (PricingPolicy) dùng để tính toán
            builder.HasOne(p => p.PricingPolicy)
                .WithMany(pp => pp.Payments)
                .HasForeignKey(p => p.PricingPolicyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
