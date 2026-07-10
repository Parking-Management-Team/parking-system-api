using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

/// <summary>
/// Cấu hình bảng ShiftReport sử dụng Fluent API của EF Core.
/// </summary>
public class ShiftReportConfiguration : IEntityTypeConfiguration<ShiftReport>
{
    public void Configure(EntityTypeBuilder<ShiftReport> builder)
    {
        // 1. Ánh xạ bảng
        builder.ToTable("shift_report");

        // 2. Khóa chính
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Id)
            .HasColumnName("shift_report_id")
            .ValueGeneratedOnAdd();

        // 3. Khóa ngoại
        builder.Property(sr => sr.StaffId)
            .HasColumnName("staff_id")
            .IsRequired();

        builder.Property(sr => sr.ApprovedById)
            .HasColumnName("approved_by_id");

        // 4. Các trường dữ liệu ca
        builder.Property(sr => sr.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(sr => sr.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(sr => sr.TotalCheckIn)
            .HasColumnName("total_check_in")
            .IsRequired();

        builder.Property(sr => sr.TotalCheckOut)
            .HasColumnName("total_check_out")
            .IsRequired();

        builder.Property(sr => sr.SystemRevenue)
            .HasColumnName("system_revenue")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sr => sr.ExpectedCashAmount)
            .HasColumnName("expected_cash_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sr => sr.ActualCashAmount)
            .HasColumnName("actual_cash_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sr => sr.DifferenceAmount)
            .HasColumnName("difference_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sr => sr.Note)
            .HasColumnName("note")
            .HasMaxLength(200);

        builder.Property(sr => sr.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue("Submitted")
            .IsRequired();

        builder.Property(sr => sr.ApprovedAt)
            .HasColumnName("approved_at");

        // 5. Audit fields
        builder.Property(sr => sr.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(sr => sr.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(sr => sr.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(sr => sr.DeletedBy)
            .HasColumnName("deleted_by");

        builder.Property(sr => sr.RowVersion)
            .IsRowVersion();

        // 6. Mối quan hệ khóa ngoại
        builder.HasOne(sr => sr.Staff)
            .WithMany()
            .HasForeignKey(sr => sr.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.ApprovedBy)
            .WithMany()
            .HasForeignKey(sr => sr.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
