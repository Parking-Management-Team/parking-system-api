using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng VehicleType (Loại phương tiện) sử dụng Fluent API của EF Core.
    /// Định nghĩa tên bảng, khóa chính, độ dài ký tự và các ràng buộc dữ liệu.
    /// </summary>
    public class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
    {
        public void Configure(EntityTypeBuilder<VehicleType> builder)
        {
            // 1. Ánh xạ thực thể VehicleType với tên bảng vật lý "vehicle_type" trong database
            builder.ToTable("vehicle_type");

            // 2. Chỉ định khóa chính của thực thể
            builder.HasKey(vt => vt.Id);

            // 3. Cấu hình cột khóa chính (Id) ánh xạ vào cột "vehicle_type_id" và tự động sinh tăng dần (Serial/Identity)
            builder.Property(vt => vt.Id)
                .HasColumnName("vehicle_type_id")
                .ValueGeneratedOnAdd();

            // 4. Tên loại xe (TypeName): tối đa 50 ký tự, bắt buộc phải nhập (NOT NULL)
            builder.Property(vt => vt.TypeName)
                .HasColumnName("type_name")
                .HasMaxLength(50)
                .IsRequired();

            // Tạo chỉ mục độc nhất (Unique Index) để ngăn trùng lặp Tên loại xe
            builder.HasIndex(vt => vt.TypeName)
                .IsUnique();

            // 5. Mô tả loại xe (Description): tối đa 100 ký tự, cho phép null
            builder.Property(vt => vt.Description)
                .HasColumnName("description")
                .HasMaxLength(100);

            // 6. Trạng thái hoạt động (VehicleTypeStatus): tối đa 20 ký tự, mặc định là "Active", bắt buộc nhập
            builder.Property(vt => vt.VehicleTypeStatus)
                .HasColumnName("vehicle_type_status")
                .HasMaxLength(20)
                .HasDefaultValue("Active")
                .IsRequired();

            // 7. Cấu hình cột RowVersion (thuộc tính kế thừa từ BaseEntity) để kiểm soát xung đột đồng thời
            builder.Property(vt => vt.RowVersion)
                .IsRowVersion();

            // 8. Thời điểm tạo bản ghi (CreatedAt - thuộc tính kế thừa từ BaseEntity)
            builder.Property(vt => vt.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        }
    }
}
