using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng Incident (Sự cố) sử dụng Fluent API.
    /// </summary>
    public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
    {
        public void Configure(EntityTypeBuilder<Incident> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("incident");

            // 2. Khóa chính
            builder.HasKey(i => i.Id);

            // 3. Cột khóa chính -> "incident_id"
            builder.Property(i => i.Id)
                .HasColumnName("incident_id")
                .ValueGeneratedOnAdd();

            // 4. Các khóa ngoại
            builder.Property(i => i.SessionId)
                .HasColumnName("session_id")
                .IsRequired();

            builder.Property(i => i.IncidentTypeId)
                .HasColumnName("incident_type_id")
                .IsRequired();

            // 5. Mô tả sự cố (Description) - tối đa 100 ký tự
            builder.Property(i => i.Description)
                .HasColumnName("description")
                .HasMaxLength(100);

            // 6. Số tiền phạt thực tế (PenaltyFee) - decimal(18,2)
            builder.Property(i => i.PenaltyFee)
                .HasColumnName("penalty_fee")
                .HasPrecision(18, 2);

            // 7. Trạng thái sự cố (IncidentStatus) - tối đa 20 ký tự, mặc định "Reported"
            builder.Property(i => i.IncidentStatus)
                .HasColumnName("incident_status")
                .HasMaxLength(20)
                .HasDefaultValue("Reported")
                .IsRequired();

            // 8. Thời điểm xử lý xong sự cố (ResolvedAt)
            builder.Property(i => i.ResolvedAt)
                .HasColumnName("resolved_at");

            // 9. Concurrency RowVersion
            builder.Property(i => i.RowVersion)
                .IsRowVersion();

            // 10. Thời điểm tạo bản ghi sự cố
            builder.Property(i => i.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều sự cố có thể phát sinh trong 1 lượt gửi xe (ParkingSession)
            builder.HasOne(i => i.Session)
                .WithMany(s => s.Incidents)
                .HasForeignKey(i => i.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều sự cố thực tế thuộc về 1 danh mục loại sự cố (IncidentType)
            builder.HasOne(i => i.IncidentType)
                .WithMany(it => it.Incidents)
                .HasForeignKey(i => i.IncidentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
