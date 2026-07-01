using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class PricingCalculationLogConfiguration : IEntityTypeConfiguration<PricingCalculationLog>
{
    public void Configure(EntityTypeBuilder<PricingCalculationLog> builder)
    {
        builder.ToTable("pricing_calculation_log");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("pricing_calculation_log_id")
            .ValueGeneratedOnAdd();

        builder.Property(l => l.BookingId)
            .HasColumnName("booking_id")
            .IsRequired(false);

        builder.Property(l => l.ParkingSessionId)
            .HasColumnName("parking_session_id")
            .IsRequired(false);

        builder.Property(l => l.VehicleTypeId)
            .HasColumnName("vehicle_type_id")
            .IsRequired();

        builder.Property(l => l.CheckInTime)
            .HasColumnName("check_in_time")
            .IsRequired();

        builder.Property(l => l.CheckOutTime)
            .HasColumnName("check_out_time")
            .IsRequired();

        builder.Property(l => l.MatchedPolicyId)
            .HasColumnName("matched_policy_id")
            .IsRequired();

        builder.Property(l => l.TotalPrice)
            .HasColumnName("total_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(l => l.CalculationDetails)
            .HasColumnName("calculation_details")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(l => l.RowVersion)
            .IsRowVersion();

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Mối quan hệ với VehicleType
        builder.HasOne(l => l.VehicleType)
            .WithMany()
            .HasForeignKey(l => l.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Mối quan hệ với PricingPolicy
        builder.HasOne(l => l.MatchedPolicy)
            .WithMany()
            .HasForeignKey(l => l.MatchedPolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes phục vụ hiệu năng tìm kiếm và đối soát nhanh
        builder.HasIndex(l => l.BookingId);
        builder.HasIndex(l => l.ParkingSessionId);
        builder.HasIndex(l => l.CreatedAt);
    }
}
