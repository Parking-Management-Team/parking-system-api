using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

public class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    public void Configure(EntityTypeBuilder<ParkingSession> builder)
    {
        builder.ToTable("parking_session", t => t.HasCheckConstraint(
            "CK_parking_session_source_exclusive",
            "booking_id IS NULL OR monthly_subscription_id IS NULL"));

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Id)
            .HasColumnName("session_id")
            .ValueGeneratedOnAdd();

        builder.Property(ps => ps.VehicleId).HasColumnName("vehicle_id").IsRequired();
        builder.Property(ps => ps.BuildingId).HasColumnName("building_id").IsRequired();
        builder.Property(ps => ps.CardId).HasColumnName("card_id").IsRequired();
        builder.Property(ps => ps.ZoneId).HasColumnName("zone_id");
        builder.Property(ps => ps.SlotId).HasColumnName("slot_id");
        builder.Property(ps => ps.BookingId).HasColumnName("booking_id");
        builder.Property(ps => ps.MonthlySubscriptionId).HasColumnName("monthly_subscription_id");
        builder.Property(ps => ps.InStaffId).HasColumnName("in_staff_id");
        builder.Property(ps => ps.OutStaffId).HasColumnName("out_staff_id");
        builder.Property(ps => ps.CheckInTime).HasColumnName("check_in_time").IsRequired();
        builder.Property(ps => ps.CheckOutTime).HasColumnName("check_out_time");
        builder.Property(ps => ps.LicensePlateIn).HasColumnName("license_plate_in").HasMaxLength(20).IsRequired();
        builder.Property(ps => ps.LicensePlateOut).HasColumnName("license_plate_out").HasMaxLength(20);
        builder.Property(ps => ps.SessionStatus)
            .HasColumnName("session_status")
            .HasMaxLength(20)
            .HasDefaultValue("ACTIVE")
            .IsRequired();
        builder.Property(ps => ps.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
        builder.Property(ps => ps.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasOne(ps => ps.Vehicle)
            .WithMany(v => v.ParkingSessions)
            .HasForeignKey(ps => ps.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Building)
            .WithMany()
            .HasForeignKey(ps => ps.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Card)
            .WithMany(c => c.ParkingSessions)
            .HasForeignKey(ps => ps.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Zone)
            .WithMany()
            .HasForeignKey(ps => ps.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.ParkingSlot)
            .WithMany(s => s.ParkingSessions)
            .HasForeignKey(ps => ps.SlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Booking)
            .WithOne()
            .HasForeignKey<ParkingSession>(ps => ps.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.MonthlySubscription)
            .WithMany()
            .HasForeignKey(ps => ps.MonthlySubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.InStaff)
            .WithMany()
            .HasForeignKey(ps => ps.InStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.OutStaff)
            .WithMany()
            .HasForeignKey(ps => ps.OutStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ps => ps.VehicleId)
            .IsUnique()
            .HasFilter("upper(session_status) = 'ACTIVE'")
            .HasDatabaseName("IX_parking_session_active_vehicle");

        builder.HasIndex(ps => ps.CardId)
            .IsUnique()
            .HasFilter("upper(session_status) = 'ACTIVE'")
            .HasDatabaseName("IX_parking_session_active_card");

        builder.HasIndex(ps => ps.SlotId)
            .IsUnique()
            .HasFilter("slot_id IS NOT NULL AND upper(session_status) = 'ACTIVE'")
            .HasDatabaseName("IX_parking_session_active_slot");

        builder.HasIndex(ps => ps.BookingId)
            .IsUnique()
            .HasFilter("booking_id IS NOT NULL")
            .HasDatabaseName("IX_parking_session_booking_id");
    }
}
