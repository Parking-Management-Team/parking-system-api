using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Infrastructure.Configurations;

/// <summary>
/// Cấu hình bảng "floor" trong database sử dụng Fluent API của EF Core.
/// Bảng này lưu thông tin các tầng trong một tòa nhà gửi xe (Building).
/// Tham chiếu SRS: §8.3.3, Table 3.6 — Physical Model: floor
/// </summary>
public class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        // 1. Ánh xạ với tên bảng vật lý "floor"
        builder.ToTable("floor");

        // 2. Khóa chính - Id kế thừa từ BaseEntity, ánh xạ thành cột "floor_id"
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("floor_id")
            .ValueGeneratedOnAdd();

        // 3. Khóa ngoại BuildingId - bắt buộc
        builder.Property(f => f.BuildingId)
            .HasColumnName("building_id")
            .IsRequired();

        // 4. Số tầng (FloorNumber) - bắt buộc
        builder.Property(f => f.FloorNumber)
            .HasColumnName("floor_number")
            .IsRequired();

        // Ràng buộc UNIQUE(building_id, floor_number) theo SRS
        builder.HasIndex(f => new { f.BuildingId, f.FloorNumber })
            .IsUnique()
            .HasDatabaseName("IX_floor_building_id_floor_number");

        // 5. Tên tầng (Name) - tùy chọn, tối đa 50 ký tự
        builder.Property(f => f.Name)
            .HasColumnName("floor_name")
            .HasMaxLength(50);

        // 6. Trạng thái tầng (Status) - lưu dưới dạng string
        builder.Property(f => f.Status)
            .HasColumnName("floor_status")
            .HasMaxLength(20)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<FloorStatus>(value, ignoreCase: true))
            .HasDefaultValue(FloorStatus.Available)
            .IsRequired();

        // 7. Thời điểm tạo bản ghi (CreatedAt - kế thừa từ BaseEntity)
        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // 8. Cấu hình RowVersion để kiểm soát xung đột đồng thời
        builder.Property(f => f.RowVersion)
            .IsRowVersion();

        // 9. Cấu hình mối quan hệ
        // Quan hệ N-1 với Building
        builder.HasOne(f => f.Building)
            .WithMany(b => b.Floors)
            .HasForeignKey(f => f.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
