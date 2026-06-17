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

        builder.Property(vt => vt.Code)
            .HasColumnName("vehicle_type_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(vt => vt.Code)
            .IsUnique()
            .HasDatabaseName("IX_vehicle_type_code");

        builder.Property(vt => vt.Name)
            .HasColumnName("vehicle_type_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(vt => vt.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(vt => vt.RowVersion)
            .IsRowVersion();
    }
}
