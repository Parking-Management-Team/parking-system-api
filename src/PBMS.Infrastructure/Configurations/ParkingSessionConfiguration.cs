using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    public void Configure(EntityTypeBuilder<ParkingSession> builder)
    {
        builder.ToTable("parking_session");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Id)
            .HasColumnName("parking_session_id")
            .ValueGeneratedOnAdd();

        builder.Property(ps => ps.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(ps => ps.ZoneId)
            .HasColumnName("zone_id");

        builder.Property(ps => ps.ParkingSlotId)
            .HasColumnName("slot_id");

        builder.Property(ps => ps.CardId)
            .HasColumnName("card_id")
            .IsRequired();

        builder.Property(ps => ps.CheckInTime)
            .HasColumnName("checkin_time")
            .IsRequired();

        builder.Property(ps => ps.CheckOutTime)
            .HasColumnName("checkout_time");

        builder.Property(ps => ps.InStaffId)
            .HasColumnName("in_staff_id");

        builder.Property(ps => ps.OutStaffId)
            .HasColumnName("out_staff_id");

        builder.Property(ps => ps.SessionStatus)
            .HasColumnName("session_status")
            .HasMaxLength(20)
            .HasDefaultValue("Active")
            .IsRequired();

        builder.Property(ps => ps.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(ps => ps.RowVersion)
            .IsRowVersion();

        builder.HasIndex(ps => ps.VehicleId)
            .HasDatabaseName("IX_parking_session_vehicle_id");

        builder.HasIndex(ps => ps.CardId)
            .HasDatabaseName("IX_parking_session_card_id");

        builder.HasOne(ps => ps.Vehicle)
            .WithMany(v => v.ParkingSessions)
            .HasForeignKey(ps => ps.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Zone)
            .WithMany()
            .HasForeignKey(ps => ps.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.ParkingSlot)
            .WithMany(s => s.ParkingSessions)
            .HasForeignKey(ps => ps.ParkingSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Card)
            .WithMany(c => c.ParkingSessions)
            .HasForeignKey(ps => ps.CardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
