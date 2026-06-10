using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng MonthlySubscription (Đăng ký vé tháng) sử dụng Fluent API.
    /// </summary>
    public class MonthlySubscriptionConfiguration : IEntityTypeConfiguration<MonthlySubscription>
    {
        public void Configure(EntityTypeBuilder<MonthlySubscription> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("monthly_subscription");

            // 2. Khóa chính
            builder.HasKey(ms => ms.Id);

            // 3. Cột khóa chính -> "monthly_subscription_id"
            builder.Property(ms => ms.Id)
                .HasColumnName("monthly_subscription_id")
                .ValueGeneratedOnAdd();

            // 4. Các khóa ngoại
            builder.Property(ms => ms.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            builder.Property(ms => ms.VehicleId)
                .HasColumnName("vehicle_id")
                .IsRequired();

            builder.Property(ms => ms.AssignedCardId)
                .HasColumnName("assigned_card_id");

            builder.Property(ms => ms.AssignedSlotId)
                .HasColumnName("assigned_slot_id");

            builder.Property(ms => ms.BuildingId)
                .HasColumnName("building_id")
                .IsRequired();

            // 5. Giá tiền tháng (MonthlyPrice) - decimal(18,2)
            builder.Property(ms => ms.MonthlyPrice)
                .HasColumnName("monthly_price")
                .HasPrecision(18, 2)
                .IsRequired();

            // 6. Các thuộc tính thời gian hiệu lực
            builder.Property(ms => ms.ActivatedAt)
                .HasColumnName("activated_at");

            builder.Property(ms => ms.ExpiredAt)
                .HasColumnName("expired_at");

            // 7. Trạng thái (MonthlySubscriptionStatus) - tối đa 20 ký tự
            builder.Property(ms => ms.MonthlySubscriptionStatus)
                .HasColumnName("monthly_subscription_status")
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .IsRequired();

            // 8. Concurrency RowVersion
            builder.Property(ms => ms.RowVersion)
                .IsRowVersion();

            // 9. Thời điểm tạo hồ sơ
            builder.Property(ms => ms.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships) và Chỉ mục (Indexes)
            // =======================================================================

            // Ràng buộc UNIQUE cho assigned_card_id (Để đảm bảo quan hệ 1-1 cho thẻ hoạt động)
            builder.HasIndex(ms => ms.AssignedCardId)
                .IsUnique()
                .HasFilter("assigned_card_id IS NOT NULL"); // Hỗ trợ nhiều giá trị NULL trên PostgreSQL

            // Quan hệ N-1: Nhiều đăng ký vé tháng thuộc sở hữu của 1 Account
            builder.HasOne(ms => ms.Account)
                .WithMany(a => a.MonthlySubscriptions)
                .HasForeignKey(ms => ms.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều đăng ký vé tháng áp dụng cho 1 Vehicle (Nhưng tại 1 thời điểm chỉ được 1 ACTIVE - logic code check)
            builder.HasOne(ms => ms.Vehicle)
                .WithMany(v => v.MonthlySubscriptions)
                .HasForeignKey(ms => ms.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ 1-1: Mỗi đăng ký vé tháng ACTIVE liên kết duy nhất với 1 Card MONTHLY
            builder.HasOne(ms => ms.AssignedCard)
                .WithOne(c => c.MonthlySubscription)
                .HasForeignKey<MonthlySubscription>(ms => ms.AssignedCardId)
                .OnDelete(DeleteBehavior.SetNull); // Nếu hủy thẻ, trả khóa ngoại về null

            // Quan hệ N-1: Nhiều đăng ký vé ô tô tháng có thể được gán chung vào 1 ParkingSlot (Ví dụ slot VIP/Monthly)
            builder.HasOne(ms => ms.AssignedSlot)
                .WithMany(ps => ps.MonthlySubscriptions)
                .HasForeignKey(ms => ms.AssignedSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều đăng ký vé tháng áp dụng tại 1 Building
            builder.HasOne(ms => ms.Building)
                .WithMany(bld => bld.MonthlySubscriptions)
                .HasForeignKey(ms => ms.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
