using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingExclusionConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql(@"
                ALTER TABLE booking
                ADD CONSTRAINT no_overlapping_slot_bookings
                EXCLUDE USING gist (
                    slot_id WITH =,
                    tstzrange(planned_checkin_time, planned_checkout_time) WITH &&
                )
                WHERE (slot_id IS NOT NULL AND (booking_status = 'Confirmed' OR booking_status = 'Pending'));
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE booking DROP CONSTRAINT IF EXISTS no_overlapping_slot_bookings;");
        }
    }
}
