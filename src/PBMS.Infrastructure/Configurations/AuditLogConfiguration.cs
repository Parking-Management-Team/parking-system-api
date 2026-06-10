using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng AuditLog (Nhật ký hệ thống) sử dụng Fluent API.
    /// </summary>
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("audit_log");

            // 2. Khóa chính
            builder.HasKey(al => al.Id);

            // 3. Cột khóa chính -> "audit_log_id"
            builder.Property(al => al.Id)
                .HasColumnName("audit_log_id")
                .ValueGeneratedOnAdd();

            // 4. Khóa ngoại liên kết tới Account (cho phép null nếu là log hệ thống)
            builder.Property(al => al.AccountId)
                .HasColumnName("account_id");

            // 5. Hành động (Action) - tối đa 50 ký tự, bắt buộc nhập
            builder.Property(al => al.Action)
                .HasColumnName("action")
                .HasMaxLength(50)
                .IsRequired();

            // 6. Bảng bị tác động (TargetTable) - tối đa 50 ký tự
            builder.Property(al => al.TargetTable)
                .HasColumnName("target_table")
                .HasMaxLength(50);

            // 7. ID bản ghi bị tác động (TargetId)
            builder.Property(al => al.TargetId)
                .HasColumnName("target_id");

            // 8. Mô tả chi tiết log (Description) - tối đa 100 ký tự
            builder.Property(al => al.Description)
                .HasColumnName("description")
                .HasMaxLength(100);

            // 9. Concurrency RowVersion
            builder.Property(al => al.RowVersion)
                .IsRowVersion();

            // 10. Thời điểm tạo log
            builder.Property(al => al.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều log được tạo bởi 1 Account
            builder.HasOne(al => al.Account)
                .WithMany(a => a.AuditLogs)
                .HasForeignKey(al => al.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
