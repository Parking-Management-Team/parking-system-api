using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng Permission (Quyền hạn hệ thống) sử dụng Fluent API.
    /// </summary>
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            // 1. Ánh xạ thực thể Permission với tên bảng vật lý "permission"
            builder.ToTable("permission");

            // 2. Chỉ định khóa chính
            builder.HasKey(p => p.Id);

            // 3. Cấu hình cột khóa chính (Id) ánh xạ vào cột "permission_id" và tự động sinh tăng dần
            builder.Property(p => p.Id)
                .HasColumnName("permission_id")
                .ValueGeneratedOnAdd();

            // 4. Mã quyền (PermissionCode): tối đa 50 ký tự, bắt buộc nhập
            builder.Property(p => p.PermissionCode)
                .HasColumnName("permission_code")
                .HasMaxLength(50)
                .IsRequired();

            // Tạo chỉ mục độc nhất (Unique Index) cho mã quyền
            builder.HasIndex(p => p.PermissionCode)
                .IsUnique();

            // 5. Tên quyền hiển thị (PermissionName): tối đa 50 ký tự, bắt buộc nhập
            builder.Property(p => p.PermissionName)
                .HasColumnName("permission_name")
                .HasMaxLength(50)
                .IsRequired();

            // 6. Mô tả chi tiết quyền (Description): tối đa 100 ký tự, cho phép null
            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(100);

            // 7. Trạng thái hoạt động (PermissionStatus): tối đa 20 ký tự, mặc định "Active", bắt buộc nhập
            builder.Property(p => p.PermissionStatus)
                .HasColumnName("permission_status")
                .HasMaxLength(20)
                .HasDefaultValue("Active")
                .IsRequired();

            // 8. Cấu hình cột RowVersion để kiểm soát xung đột đồng thời
            builder.Property(p => p.RowVersion)
                .IsRowVersion();

            // 9. Thời điểm tạo bản ghi (CreatedAt)
            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        }
    }
}
