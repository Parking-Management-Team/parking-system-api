using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingSystemConfigAndZoneBookingLimitRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "booking_limit_rate",
                table: "zone",
                type: "integer",
                nullable: false,
                defaultValue: 80);

            migrationBuilder.CreateTable(
                name: "parking_system_configs",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parking_system_configs", x => x.Key);
                });

            migrationBuilder.InsertData(
                table: "parking_system_configs",
                columns: new[] { "Key", "Description", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { "BUFFER_TIME_MINUTES", "Buffer time in minutes between consecutive bookings on the same slot.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "30" },
                    { "WALKIN_STAY_THRESHOLD_HOURS", "Hours threshold: if booking starts within this many hours from now, walk-in car count is included in zone capacity check.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "2" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parking_system_configs");

            migrationBuilder.DropColumn(
                name: "booking_limit_rate",
                table: "zone");
        }
    }
}
