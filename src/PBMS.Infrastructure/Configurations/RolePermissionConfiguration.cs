using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng trung gian RolePermission (Mối quan hệ Nhiều-Nhiều giữa Role và Permission).
    /// Thiết lập khóa chính hợp phần (Composite Key) và các ràng buộc khóa ngoại thích hợp.
    /// </summary>
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            // 1. Ánh xạ thực thể với tên bảng vật lý "role_permission" trong database
            builder.ToTable("role_permission");

            // 2. Định nghĩa khóa chính hợp phần (Composite PK) gồm: role_id và permission_id
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // 3. Cấu hình tên cột trong database cho các thuộc tính khóa ngoại
            builder.Property(rp => rp.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            builder.Property(rp => rp.PermissionId)
                .HasColumnName("permission_id")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều RolePermission thuộc về 1 Role (Vai trò)
            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade); // Khi xóa Role, tự động xóa liên kết của nó

            // Quan hệ N-1: Nhiều RolePermission thuộc về 1 Permission (Quyền)
            builder.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade); // Khi xóa Permission, tự động xóa liên kết của nó
        }
    }
}
