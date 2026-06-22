using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng IncidentType (Loại sự cố) sử dụng Fluent API.
    /// </summary>
    public class IncidentTypeConfiguration : IEntityTypeConfiguration<IncidentType>
    {
        public void Configure(EntityTypeBuilder<IncidentType> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("incident_type");

            // 2. Khóa chính
            builder.HasKey(it => it.Id);

            // 3. Cột khóa chính -> "incident_type_id"
            builder.Property(it => it.Id)
                .HasColumnName("incident_type_id")
                .ValueGeneratedOnAdd();

            // 4. Mã sự cố (IncidentCode) - tối đa 20 ký tự, bắt buộc nhập
            builder.Property(it => it.IncidentCode)
                .HasColumnName("incident_code")
                .HasMaxLength(20)
                .IsRequired();

            // Tạo chỉ mục độc nhất (Unique Index) cho mã loại sự cố
            builder.HasIndex(it => it.IncidentCode)
                .IsUnique();

            // 5. Tên loại sự cố (IncidentName) - tối đa 50 ký tự, bắt buộc nhập
            builder.Property(it => it.IncidentName)
                .HasColumnName("incident_name")
                .HasMaxLength(50)
                .IsRequired();

            // 6. Mô tả loại sự cố (Description) - tối đa 100 ký tự, cho phép null
            builder.Property(it => it.Description)
                .HasColumnName("description")
                .HasMaxLength(100);



            // 8. Concurrency RowVersion
            builder.Property(it => it.RowVersion)
                .IsRowVersion();

            // 9. Thời điểm tạo loại sự cố
            builder.Property(it => it.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        }
    }
}
