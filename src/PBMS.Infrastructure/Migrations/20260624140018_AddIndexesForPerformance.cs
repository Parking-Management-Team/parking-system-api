using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_parking_session_status",
                table: "parking_session",
                column: "session_status");

            migrationBuilder.CreateIndex(
                name: "IX_booking_booking_status",
                table: "booking",
                column: "booking_status");

            migrationBuilder.CreateIndex(
                name: "IX_booking_checkin_grace_until",
                table: "booking",
                column: "checkin_grace_until");

            migrationBuilder.CreateIndex(
                name: "IX_booking_payment_deadline",
                table: "booking",
                column: "payment_deadline");

            migrationBuilder.CreateIndex(
                name: "IX_booking_planned_checkout_time",
                table: "booking",
                column: "planned_checkout_time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_parking_session_status",
                table: "parking_session");

            migrationBuilder.DropIndex(
                name: "IX_booking_booking_status",
                table: "booking");

            migrationBuilder.DropIndex(
                name: "IX_booking_checkin_grace_until",
                table: "booking");

            migrationBuilder.DropIndex(
                name: "IX_booking_payment_deadline",
                table: "booking");

            migrationBuilder.DropIndex(
                name: "IX_booking_planned_checkout_time",
                table: "booking");
        }
    }
}
