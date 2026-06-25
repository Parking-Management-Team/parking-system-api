using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotIdToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "slot_id",
                table: "booking",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_slot_id",
                table: "booking",
                column: "slot_id");

            migrationBuilder.AddForeignKey(
                name: "FK_booking_parking_slot_slot_id",
                table: "booking",
                column: "slot_id",
                principalTable: "parking_slot",
                principalColumn: "slot_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_booking_parking_slot_slot_id",
                table: "booking");

            migrationBuilder.DropIndex(
                name: "IX_booking_slot_id",
                table: "booking");

            migrationBuilder.DropColumn(
                name: "slot_id",
                table: "booking");
        }
    }
}
