using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class ParkingSystemConfigConfiguration : IEntityTypeConfiguration<ParkingSystemConfig>
{
    public void Configure(EntityTypeBuilder<ParkingSystemConfig> builder)
    {
        builder.ToTable("parking_system_configs");

        builder.HasKey(c => c.Key);

        builder.Property(c => c.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Value)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(100);

        // Seed default values
        builder.HasData(
            new ParkingSystemConfig
            {
                Key = "BUFFER_TIME_MINUTES",
                Value = "30",
                Description = "Buffer time in minutes between consecutive bookings on the same slot.",
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ParkingSystemConfig
            {
                Key = "WALKIN_STAY_THRESHOLD_HOURS",
                Value = "2",
                Description = "Hours threshold: if booking starts within this many hours from now, walk-in car count is included in zone capacity check.",
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
