using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Infrastructure.Configurations;

/// <summary>
/// Cấu hình bảng "parking_slot" trong database sử dụng Fluent API của EF Core.
/// Tham chiếu SRS: §8.3.3, Table 3.9 — Physical Model: parking_slot
/// </summary>
public class ParkingSlotConfiguration : IEntityTypeConfiguration<ParkingSlot>
{
    public void Configure(EntityTypeBuilder<ParkingSlot> builder)
    {
        // 1. Ánh xạ với tên bảng vật lý "parking_slot"
        builder.ToTable("parking_slot");

        // 2. Khóa chính - Id kế thừa từ BaseEntity, ánh xạ thành cột "slot_id"
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("slot_id")
            .ValueGeneratedOnAdd();

        // 3. Khóa ngoại ZoneId - bắt buộc
        builder.Property(s => s.ZoneId)
            .HasColumnName("zone_id")
            .IsRequired();

        // 4. Khóa ngoại VehicleTypeId - bắt buộc
        builder.Property(s => s.VehicleTypeId)
            .HasColumnName("vehicle_type_id")
            .IsRequired();

        // 5. Mã slot (Code) - bắt buộc, tối đa 20 ký tự, UNIQUE
        builder.Property(s => s.Code)
            .HasColumnName("slot_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasDatabaseName("IX_parking_slot_slot_code");

        // 6. Tên hiển thị (Name) - tùy chọn, tối đa 50 ký tự
        builder.Property(s => s.Name)
            .HasColumnName("slot_name")
            .HasMaxLength(50);

        // 7. Trạng thái slot (Status) - lưu dưới dạng string
        builder.Property(s => s.Status)
            .HasColumnName("slot_status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(SlotStatus.Available)
            .IsRequired();

        // 8. Thời điểm tạo bản ghi (CreatedAt - kế thừa từ BaseEntity)
        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // 9. Cột RowVersion (kế thừa từ BaseEntity) - kiểm soát xung đột đồng thời
        builder.Property(s => s.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // 10. Cấu hình mối quan hệ
        // Quan hệ N-1 với Zone
        builder.HasOne(s => s.Zone)
            .WithMany(z => z.ParkingSlots)
            .HasForeignKey(s => s.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        // Quan hệ N-1 với VehicleType
        builder.HasOne(s => s.VehicleType)
            .WithMany()
            .HasForeignKey(s => s.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
