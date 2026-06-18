using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Infrastructure.Configurations;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("zone");

        builder.HasKey(z => z.Id);

        builder.Property(z => z.Id)
            .HasColumnName("zone_id")
            .ValueGeneratedOnAdd();

        builder.Property(z => z.FloorId)
            .HasColumnName("floor_id")
            .IsRequired();

        builder.Property(z => z.Code)
            .HasColumnName("zone_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(z => new { z.FloorId, z.Code })
            .IsUnique()
            .HasDatabaseName("IX_zone_floor_id_zone_code");

        builder.Property(z => z.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(z => z.VehicleTypeId)
            .HasColumnName("vehicle_type_id")
            .IsRequired();

        builder.Property(z => z.Capacity)
            .HasColumnName("capacity")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(z => z.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<ZoneStatus>(value, ignoreCase: true))
            .HasDefaultValue(ZoneStatus.Available)
            .IsRequired();

        builder.Property(z => z.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(z => z.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasOne(z => z.Floor)
            .WithMany(f => f.Zones)
            .HasForeignKey(z => z.FloorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(z => z.VehicleType)
            .WithMany()
            .HasForeignKey(z => z.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
