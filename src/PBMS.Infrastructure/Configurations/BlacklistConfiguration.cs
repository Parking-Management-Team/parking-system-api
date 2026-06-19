using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng Blacklist (Danh sách đen chặn xe/thẻ) sử dụng Fluent API.
    /// </summary>
    public class BlacklistConfiguration : IEntityTypeConfiguration<Blacklist>
    {
        public void Configure(EntityTypeBuilder<Blacklist> builder)
        {
            // 1. Ánh xạ bảng vật lý và cấu hình ràng buộc kiểm tra CHECK
            builder.ToTable("blacklist", t => t.HasCheckConstraint(
                "CK_Blacklist_Source",
                "vehicle_id IS NOT NULL OR card_id IS NOT NULL OR incident_id IS NOT NULL"
            ));

            // 2. Khóa chính
            builder.HasKey(b => b.Id);

            // 3. Cột khóa chính -> "blacklist_id"
            builder.Property(b => b.Id)
                .HasColumnName("blacklist_id")
                .ValueGeneratedOnAdd();

            // 4. Các khóa ngoại (cho phép null vì có thể chặn theo xe, thẻ, hoặc phát sinh từ sự cố)
            builder.Property(b => b.VehicleId)
                .HasColumnName("vehicle_id");

            builder.Property(b => b.CardId)
                .HasColumnName("card_id");

            builder.Property(b => b.IncidentId)
                .HasColumnName("incident_id");

            // 5. Lý do chặn (Reason) - tối đa 100 ký tự, bắt buộc nhập
            builder.Property(b => b.Reason)
                .HasColumnName("reason")
                .HasMaxLength(100)
                .IsRequired();

            // 6. Concurrency RowVersion
            builder.Property(b => b.RowVersion)
                .IsRowVersion();

            // 7. Thời điểm tạo bản ghi chặn
            builder.Property(b => b.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // 8. Soft Delete columns
            builder.Property(b => b.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(b => b.DeletedAt)
                .HasColumnName("deleted_at");

            builder.Property(b => b.DeletedBy)
                .HasColumnName("deleted_by");

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều bản ghi chặn có thể liên kết tới 1 Vehicle
            builder.HasOne(b => b.Vehicle)
                .WithMany(v => v.Blacklists)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều bản ghi chặn có thể liên kết tới 1 Card
            builder.HasOne(b => b.Card)
                .WithMany(c => c.Blacklists)
                .HasForeignKey(b => b.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều bản ghi chặn có thể bắt nguồn từ 1 Incident (Sự cố)
            builder.HasOne(b => b.Incident)
                .WithMany(i => i.Blacklists)
                .HasForeignKey(b => b.IncidentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
