using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Infrastructure.Configurations;

/// <summary>
/// Cấu hình bảng "zone" trong database sử dụng Fluent API của EF Core.
/// Bảng này lưu thông tin các khu vực gửi xe trong một tầng (Floor).
/// </summary>
public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        // 1. Ánh xạ với tên bảng vật lý "zone"
        builder.ToTable("zone");

        // 2. Khóa chính - Id kế thừa từ BaseEntity, ánh xạ thành cột "zone_id"
        builder.HasKey(z => z.Id);

        builder.Property(z => z.Id)
            .HasColumnName("zone_id")
            .ValueGeneratedOnAdd();

        // 3. Khóa ngoại FloorId - bắt buộc
        builder.Property(z => z.FloorId)
            .HasColumnName("floor_id")
            .IsRequired();

        builder.Property(z => z.Code)
            .HasColumnName("zone_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(z => new { z.FloorId, z.Code })
            .IsUnique()
            .HasDatabaseName("IX_zone_floor_id_zone_code");

        // 4. Tên khu vực (Name) - bắt buộc, tối đa 50 ký tự
        builder.Property(z => z.Name)
            .HasColumnName("zone_name")
            .HasMaxLength(50)
            .IsRequired();

        // 5. Khóa ngoại VehicleTypeId - bắt buộc
        builder.Property(z => z.VehicleTypeId)
            .HasColumnName("vehicle_type_id")
            .IsRequired();

        // 6. Sức chứa (Capacity) - mặc định là 0
        builder.Property(z => z.Capacity)
            .HasColumnName("capacity")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(z => z.ZoneAccessType)
            .HasColumnName("zone_access_type")
            .HasMaxLength(20)
            .HasDefaultValue("GENERAL")
            .IsRequired();

        // 7. Trạng thái khu vực (Status) - lưu dưới dạng string
        builder.Property(z => z.Status)
            .HasColumnName("zone_status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(ZoneStatus.Available)
            .IsRequired();

        // 8. Thời điểm tạo bản ghi (CreatedAt - kế thừa từ BaseEntity)
        builder.Property(z => z.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // 9. Cấu hình RowVersion để kiểm soát xung đột đồng thời
        builder.Property(z => z.RowVersion)
            .IsRowVersion();

        // 10. Cấu hình mối quan hệ
        // Quan hệ N-1 với Floor
        builder.HasOne(z => z.Floor)
            .WithMany(f => f.Zones)
            .HasForeignKey(z => z.FloorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Quan hệ N-1 với VehicleType
        builder.HasOne(z => z.VehicleType)
            .WithMany()
            .HasForeignKey(z => z.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
