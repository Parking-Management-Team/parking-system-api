using PBMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PBMS.Infrastructure.Configurations;

/// <summary>
/// Configuration for ParkingSession entity using Fluent API.
/// </summary>
public class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    public void Configure(EntityTypeBuilder<ParkingSession> builder)
    {
        // Table name
        builder.ToTable("parking_sessions");

        // Primary key
        builder.HasKey(ps => ps.Id);

        // Foreign key and relationship
        builder.HasOne(ps => ps.Vehicle)
            .WithMany()
            .HasForeignKey(ps => ps.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Properties
        builder.Property(ps => ps.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ps => ps.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(ps => ps.VehicleId)
            .HasDatabaseName("ix_parking_sessions_vehicle_id");

        builder.HasIndex(ps => ps.IsCompleted)
            .HasDatabaseName("ix_parking_sessions_is_completed");
    }
}
