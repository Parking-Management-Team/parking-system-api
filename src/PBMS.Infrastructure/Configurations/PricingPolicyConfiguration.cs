using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng PricingPolicy (Chính sách giá) sử dụng Fluent API.
    /// </summary>
    public class PricingPolicyConfiguration : IEntityTypeConfiguration<PricingPolicy>
    {
        public void Configure(EntityTypeBuilder<PricingPolicy> builder)
        {
            // 1. Ánh xạ bảng vật lý
            builder.ToTable("pricing_policy");

            // 2. Khóa chính
            builder.HasKey(pp => pp.Id);

            // 3. Cột khóa chính
            builder.Property(pp => pp.Id)
                .HasColumnName("pricing_policy_id")
                .ValueGeneratedOnAdd();

            // 4. Khóa ngoại VehicleTypeId
            builder.Property(pp => pp.VehicleTypeId)
                .HasColumnName("vehicle_type_id")
                .IsRequired();

            // 5. Tên chính sách (PolicyName)
            builder.Property(pp => pp.PolicyName)
                .HasColumnName("policy_name")
                .HasMaxLength(100)
                .IsRequired();

            // 6. Ngày hiệu lực bắt đầu (EffectiveStart) - lưu kiểu date trong DB
            builder.Property(pp => pp.EffectiveStart)
                .HasColumnName("effective_start")
                .HasColumnType("date")
                .IsRequired();

            // 7. Ngày kết thúc hiệu lực (EffectiveEnd) - lưu kiểu date trong DB, có thể null
            builder.Property(pp => pp.EffectiveEnd)
                .HasColumnName("effective_end")
                .HasColumnType("date");

            // 8. Trạng thái (PricingPolicyStatus)
            builder.Property(pp => pp.PricingPolicyStatus)
                .HasColumnName("pricing_policy_status")
                .HasMaxLength(20)
                .HasDefaultValue("Active")
                .IsRequired();

            // 9. Concurrency RowVersion
            builder.Property(pp => pp.RowVersion)
                .IsRowVersion();

            // 10. Thời điểm tạo
            builder.Property(pp => pp.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều chính sách giá thuộc về 1 loại phương tiện (VehicleType)
            builder.HasOne(pp => pp.VehicleType)
                .WithMany(vt => vt.PricingPolicies)
                .HasForeignKey(pp => pp.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
