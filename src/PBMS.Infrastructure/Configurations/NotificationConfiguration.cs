using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng Notification (Thông báo) sử dụng Fluent API.
    /// </summary>
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("notification");

            // 2. Khóa chính
            builder.HasKey(n => n.Id);

            // 3. Cột khóa chính -> "notification_id"
            builder.Property(n => n.Id)
                .HasColumnName("notification_id")
                .ValueGeneratedOnAdd();

            // 4. Khóa ngoại liên kết tới Account
            builder.Property(n => n.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            // 5. Tiêu đề thông báo (Title) - tối đa 100 ký tự, bắt buộc nhập
            builder.Property(n => n.Title)
                .HasColumnName("title")
                .HasMaxLength(100)
                .IsRequired();

            // 6. Nội dung thông báo (Message) - tối đa 100 ký tự, bắt buộc nhập
            builder.Property(n => n.Message)
                .HasColumnName("message")
                .HasMaxLength(100)
                .IsRequired();

            // 7. Concurrency RowVersion
            builder.Property(n => n.RowVersion)
                .IsRowVersion();

            // 8. Thời điểm gửi thông báo
            builder.Property(n => n.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều thông báo được gửi tới 1 Account
            builder.HasOne(n => n.Account)
                .WithMany(a => a.Notifications)
                .HasForeignKey(n => n.AccountId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa tài khoản sẽ tự động xóa tất cả thông báo của tài khoản đó
        }
    }
}
