using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
{
    public void Configure(EntityTypeBuilder<VehicleType> builder)
    {
        builder.ToTable("vehicle_type");

        builder.HasKey(vt => vt.Id);

        builder.Property(vt => vt.Id)
            .HasColumnName("vehicle_type_id")
            .ValueGeneratedOnAdd();

        builder.Property(vt => vt.TypeName)
            .HasColumnName("vehicle_type_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(vt => vt.VehicleTypeCode)
            .HasColumnName("vehicle_type_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(vt => vt.TypeName)
            .IsUnique()
            .HasDatabaseName("IX_vehicle_type_type_name");

        builder.Property(vt => vt.Description)
            .HasColumnName("description")
            .HasMaxLength(100);

        builder.Property(vt => vt.VehicleTypeStatus)
            .HasColumnName("vehicle_type_status")
            .HasMaxLength(20)
            .HasDefaultValue(VehicleType.StatusActive)
            .IsRequired();

        builder.Property(vt => vt.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(vt => vt.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();
    }
}
