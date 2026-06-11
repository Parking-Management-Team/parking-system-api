using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng Role (Vai trò hệ thống) sử dụng Fluent API của EF Core.
    /// Định nghĩa tên bảng, khóa chính, độ dài ký tự và các ràng buộc dữ liệu.
    /// </summary>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // 1. Ánh xạ thực thể Role với tên bảng vật lý "role" trong database
            builder.ToTable("role");

            // 2. Chỉ định khóa chính của thực thể
            builder.HasKey(r => r.Id);

            // 3. Cấu hình cột khóa chính (Id) ánh xạ vào cột "role_id" và tự động sinh tăng dần (Serial/Identity)
            builder.Property(r => r.Id)
                .HasColumnName("role_id")
                .ValueGeneratedOnAdd();

            // 4. Tên vai trò (RoleName): tối đa 50 ký tự, bắt buộc phải nhập (NOT NULL)
            builder.Property(r => r.RoleName)
                .HasColumnName("role_name")
                .HasMaxLength(50)
                .IsRequired();

            // Tạo chỉ mục độc nhất (Unique Index) để ngăn trùng lặp Tên vai trò
            builder.HasIndex(r => r.RoleName)
                .IsUnique();

            // 5. Mô tả vai trò (Description): tối đa 100 ký tự, cho phép null
            builder.Property(r => r.Description)
                .HasColumnName("description")
                .HasMaxLength(100);

            // 6. Cấu hình cột RowVersion (thuộc tính kế thừa từ BaseEntity):
            // Dùng để kiểm soát xung đột đồng thời (Concurrency Control).
            // Khi Update/Delete, EF Core sẽ kiểm tra giá trị này khớp với DB hay không.
            // Nếu không khớp → nghĩa là bản ghi đã bị người khác sửa → ném DbUpdateConcurrencyException.
            builder.Property(r => r.RowVersion)
                .IsRowVersion();

            // 7. Thời điểm tạo bản ghi (CreatedAt - thuộc tính kế thừa từ BaseEntity):
            // Ánh xạ cột "created_at", mặc định lấy giờ hệ thống database (PostgreSQL: CURRENT_TIMESTAMP)
            builder.Property(r => r.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        }
    }
}
