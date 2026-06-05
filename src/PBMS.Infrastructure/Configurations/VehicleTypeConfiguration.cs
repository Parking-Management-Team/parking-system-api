using PBMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PBMS.Infrastructure.Configurations;

/// <summary>
/// Configuration for VehicleType entity using Fluent API.
/// </summary>
public class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
{
    public void Configure(EntityTypeBuilder<VehicleType> builder)
    {
        // Table name
        builder.ToTable("vehicle_types");

        // Primary key
        builder.HasKey(vt => vt.Id);

        // Properties
        builder.Property(vt => vt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(vt => vt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(vt => vt.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(vt => vt.Name)
            .IsUnique()
            .HasDatabaseName("ix_vehicle_types_name");

        // Seed data
        builder.HasData(
            new VehicleType
            {
                Id = 1,
                Name = "Motorcycle",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new VehicleType
            {
                Id = 2,
                Name = "Car",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}

