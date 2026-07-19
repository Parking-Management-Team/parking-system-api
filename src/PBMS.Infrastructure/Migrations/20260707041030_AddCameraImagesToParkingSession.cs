using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCameraImagesToParkingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_in",
                table: "parking_session",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_out",
                table: "parking_session",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_in",
                table: "parking_session");

            migrationBuilder.DropColumn(
                name: "image_out",
                table: "parking_session");
        }
    }
}
