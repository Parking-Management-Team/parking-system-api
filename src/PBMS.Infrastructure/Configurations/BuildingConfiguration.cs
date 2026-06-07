using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Infrastructure.Configurations;

/// <summary>
/// Cấu hình bảng "building" trong database sử dụng Fluent API của EF Core.
/// Tham chiếu SRS: §8.3.3, Table 3.5 — Physical Model: building
/// </summary>
public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        // 1. Ánh xạ với tên bảng vật lý "building"
        builder.ToTable("building");

        // 2. Khóa chính - Id kế thừa từ BaseEntity, ánh xạ thành cột "building_id"
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("building_id")
            .ValueGeneratedOnAdd();

        // 3. Mã tòa nhà (Code) - bắt buộc, tối đa 20 ký tự, UNIQUE
        builder.Property(b => b.Code)
            .HasColumnName("building_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(b => b.Code)
            .IsUnique()
            .HasDatabaseName("IX_building_building_code");

        // 4. Tên tòa nhà (Name) - bắt buộc, tối đa 50 ký tự
        builder.Property(b => b.Name)
            .HasColumnName("building_name")
            .HasMaxLength(50)
            .IsRequired();

        // 5. Địa chỉ (Address) - tùy chọn, tối đa 100 ký tự
        builder.Property(b => b.Address)
            .HasColumnName("address")
            .HasMaxLength(100);

        // 6. Tổng số tầng (TotalFloor) - bắt buộc
        builder.Property(b => b.TotalFloor)
            .HasColumnName("total_floor")
            .IsRequired();

        // 7. Trạng thái tòa nhà (Status) - lưu dưới dạng string
        builder.Property(b => b.Status)
            .HasColumnName("building_status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(BuildingStatus.Available)
            .IsRequired();

        // 8. Thời điểm tạo bản ghi (CreatedAt - kế thừa từ BaseEntity)
        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // 9. Cột RowVersion (kế thừa từ BaseEntity) - kiểm soát xung đột đồng thời
        builder.Property(b => b.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // 10. Cấu hình mối quan hệ
        // Quan hệ 1-N với Floor
        builder.HasMany(b => b.Floors)
            .WithOne(f => f.Building)
            .HasForeignKey(f => f.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
