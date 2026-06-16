using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng Booking (Đặt chỗ trước) sử dụng Fluent API.
    /// </summary>
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("booking");

            // 2. Khóa chính
            builder.HasKey(b => b.Id);

            // 3. Cấu hình cột khóa chính (Id) -> "booking_id"
            builder.Property(b => b.Id)
                .HasColumnName("booking_id")
                .ValueGeneratedOnAdd();

            // 4. Khóa ngoại liên kết
            builder.Property(b => b.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            builder.Property(b => b.VehicleId)
                .HasColumnName("vehicle_id")
                .IsRequired();

            builder.Property(b => b.VehicleTypeId)
                .HasColumnName("vehicle_type_id")
                .IsRequired();

            builder.Property(b => b.BuildingId)
                .HasColumnName("building_id")
                .IsRequired();

            // 5. Các cột thời gian
            builder.Property(b => b.PlannedCheckinTime)
                .HasColumnName("planned_checkin_time")
                .IsRequired();

            builder.Property(b => b.PlannedCheckoutTime)
                .HasColumnName("planned_checkout_time")
                .IsRequired();

            builder.Property(b => b.PaymentDeadline)
                .HasColumnName("payment_deadline")
                .IsRequired();

            builder.Property(b => b.CheckinGraceUntil)
                .HasColumnName("checkin_grace_until")
                .IsRequired();

            builder.Property(b => b.CancelledAt)
                .HasColumnName("cancelled_at");

            builder.Property(b => b.ConfirmedAt)
                .HasColumnName("confirmed_at");

            // 6. Số tiền cọc (DepositAmount) - decimal(18,2)
            builder.Property(b => b.DepositAmount)
                .HasColumnName("deposit_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            // 7. Trạng thái (BookingStatus) - tối đa 20 ký tự
            builder.Property(b => b.BookingStatus)
                .HasColumnName("booking_status")
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .IsRequired();

            // 8. Lý do hủy (CancelReason) - tối đa 100 ký tự
            builder.Property(b => b.CancelReason)
                .HasColumnName("cancel_reason")
                .HasMaxLength(100);

            // 9. Concurrency RowVersion
            builder.Property(b => b.RowVersion)
                .IsRowVersion();

            // 10. Thời điểm tạo
            builder.Property(b => b.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều Booking được tạo bởi 1 Account
            builder.HasOne(b => b.Account)
                .WithMany(a => a.Bookings)
                .HasForeignKey(b => b.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều Booking áp dụng cho 1 Vehicle
            builder.HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều Booking áp dụng cho 1 VehicleType
            builder.HasOne(b => b.VehicleType)
                .WithMany(vt => vt.Bookings)
                .HasForeignKey(b => b.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều Booking diễn ra tại 1 Building
            builder.HasOne(b => b.Building)
                .WithMany(bld => bld.Bookings)
                .HasForeignKey(b => b.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
