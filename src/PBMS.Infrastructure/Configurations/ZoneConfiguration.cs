using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Infrastructure.Configurations;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("zone");

        builder.HasKey(z => z.Id);

        builder.Property(z => z.Id)
            .HasColumnName("zone_id")
            .ValueGeneratedOnAdd();

        builder.Property(z => z.FloorId)
            .HasColumnName("floor_id")
            .IsRequired();

        // 4. Mã khu vực (Code) - bắt buộc, tối đa 20 ký tự
        builder.Property(z => z.Code)
            .HasColumnName("zone_code")
            .HasMaxLength(20)
            .IsRequired();

        // Ràng buộc UNIQUE(floor_id, zone_code) theo SRS
        builder.HasIndex(z => new { z.FloorId, z.Code })
            .IsUnique()
            .HasDatabaseName("IX_zone_floor_id_zone_code");

        // 5. Tên khu vực (Name) - bắt buộc, tối đa 50 ký tự
        builder.Property(z => z.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        // 6. Khóa ngoại VehicleTypeId - bắt buộc
        builder.Property(z => z.VehicleTypeId)
            .HasColumnName("vehicle_type_id")
            .IsRequired();

        // 7. Sức chứa (Capacity) - mặc định là 0
        builder.Property(z => z.Capacity)
            .HasColumnName("capacity")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(z => z.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<ZoneStatus>(value, ignoreCase: true))
            .HasDefaultValue(ZoneStatus.Available)
            .IsRequired();

        builder.Property(z => z.AccessType)
            .HasColumnName("zone_access_type")
            .HasMaxLength(20)
            .HasConversion(
                type => type.ToString(),
                value => Enum.Parse<ZoneAccessType>(value, ignoreCase: true))
            .HasDefaultValue(ZoneAccessType.General)
            .IsRequired();


        // 10. Thời điểm tạo bản ghi (CreatedAt - kế thừa từ BaseEntity)
        builder.Property(z => z.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // 11. Cấu hình RowVersion để kiểm soát xung đột đồng thời
        builder.Property(z => z.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        // 12. Cấu hình mối quan hệ
        // Quan hệ N-1 với Floor
        builder.HasOne(z => z.Floor)
            .WithMany(f => f.Zones)
            .HasForeignKey(z => z.FloorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(z => z.VehicleType)
            .WithMany()
            .HasForeignKey(z => z.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
