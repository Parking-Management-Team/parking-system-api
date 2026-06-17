using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicle");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("vehicle_id")
            .ValueGeneratedOnAdd();

        builder.Property(v => v.VehicleTypeId)
            .HasColumnName("vehicle_type_id")
            .IsRequired();

        builder.Property(v => v.LicensePlate)
            .HasColumnName("license_plate")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(v => v.LicensePlate)
            .IsUnique()
            .HasDatabaseName("IX_vehicle_license_plate");

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(v => v.RowVersion)
            .IsRowVersion();

        builder.HasOne(v => v.VehicleType)
            .WithMany(vt => vt.Vehicles)
            .HasForeignKey(v => v.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
