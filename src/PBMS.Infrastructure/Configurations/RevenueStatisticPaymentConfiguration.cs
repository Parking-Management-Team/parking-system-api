using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng trung gian RevenueStatisticPayment sử dụng Fluent API.
    /// Thiết lập mối quan hệ Nhiều-Nhiều (N-N) giữa RevenueStatistic và Payment.
    /// </summary>
    public class RevenueStatisticPaymentConfiguration : IEntityTypeConfiguration<RevenueStatisticPayment>
    {
        public void Configure(EntityTypeBuilder<RevenueStatisticPayment> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("revenue_statistic_payment");

            // 2. Khóa chính hợp phần (Composite PK)
            builder.HasKey(rsp => new { rsp.StatisticId, rsp.PaymentId });

            // 3. Khai báo cột cho các thuộc tính
            builder.Property(rsp => rsp.StatisticId)
                .HasColumnName("statistic_id")
                .IsRequired();

            builder.Property(rsp => rsp.PaymentId)
                .HasColumnName("payment_id")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều dòng liên kết trung gian thuộc về 1 dòng RevenueStatistic
            builder.HasOne(rsp => rsp.RevenueStatistic)
                .WithMany(rs => rs.RevenueStatisticPayments)
                .HasForeignKey(rsp => rsp.StatisticId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa thống kê thì tự động xóa liên kết của nó

            // Quan hệ N-1: Nhiều dòng liên kết trung gian thuộc về 1 giao dịch Payment
            builder.HasOne(rsp => rsp.Payment)
                .WithMany(p => p.RevenueStatisticPayments)
                .HasForeignKey(rsp => rsp.PaymentId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa thanh toán thì tự động xóa liên kết của nó
        }
    }
}
