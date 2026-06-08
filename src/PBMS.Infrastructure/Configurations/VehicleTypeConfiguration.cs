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
        builder.ToTable("vehicle_type");

        builder.HasKey(vt => vt.Id);

        builder.Property(vt => vt.Id)
            .HasColumnName("vehicle_type_id");

        builder.Property(vt => vt.Name)
            .HasColumnName("type_name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(vt => vt.Description)
            .HasColumnName("description")
            .HasMaxLength(100);

        builder.Property(vt => vt.VehicleTypeStatus)
            .HasColumnName("vehicle_type_status")
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(VehicleType.StatusActive);

        builder.Ignore(vt => vt.CreatedAt);

        builder.HasIndex(vt => vt.Name)
            .IsUnique()
            .HasDatabaseName("ix_vehicle_type_type_name");

        builder.HasData(
            new VehicleType
            {
                Id = 1,
                Name = VehicleType.MotorcycleTypeName,
                Description = "Managed by zone capacity. Slot is not required for booking or monthly card.",
                VehicleTypeStatus = VehicleType.StatusActive
            },
            new VehicleType
            {
                Id = 2,
                Name = VehicleType.CarTypeName,
                Description = "Managed by slot for booking and monthly card.",
                VehicleTypeStatus = VehicleType.StatusActive
            }
        );
    }
}

