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
        builder.ToTable("ParkingSessions");

        // Primary key
        builder.HasKey(ps => ps.Id);

        // Foreign key and relationship
        builder.HasOne(ps => ps.Vehicle)
            .WithMany(v => v.ParkingSessions)
            .HasForeignKey(ps => ps.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(ps => ps.IsCompleted);

        builder.Property(ps => ps.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(ps => ps.VehicleId)
            .HasDatabaseName("IX_ParkingSessions_VehicleId");
    }
}
