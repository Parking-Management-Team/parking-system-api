using PBMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PBMS.Infrastructure.Configurations;

/// <summary>
/// Configuration for Vehicle entity using Fluent API.
/// </summary>
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        // Table name
        builder.ToTable("vehicles");

        // Primary key
        builder.HasKey(v => v.Id);

        // Foreign key and relationship
        builder.HasOne(v => v.VehicleType)
            .WithMany()
            .HasForeignKey(v => v.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Properties
        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(v => v.VehicleTypeId)
            .HasDatabaseName("ix_vehicles_vehicle_type_id");
    }
}
