using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLostAtToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_zone_capacity",
                table: "zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "lost_at",
                table: "card",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lost_at",
                table: "card");

            migrationBuilder.AddCheckConstraint(
                name: "CK_zone_capacity",
                table: "zone",
                sql: "capacity >= 0");
        }
    }
}
