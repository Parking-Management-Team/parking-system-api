using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateParkingStructure_AlignSRS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_card_CardId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_vehicle_VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_zone_floor_id",
                table: "zone");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "zone",
                newName: "zone_code");

            migrationBuilder.AlterColumn<string>(
                name: "zone_code",
                table: "zone",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "zone_access_type",
                table: "zone",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "General");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "ParkingSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CardId",
                table: "ParkingSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuildingId",
                table: "ParkingSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "ParkingSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutTime",
                table: "ParkingSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InStaffId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicensePlateIn",
                table: "ParkingSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicensePlateOut",
                table: "ParkingSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonthlySubscriptionId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutStaffId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlotId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "floor_status",
                table: "floor",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Available");

            migrationBuilder.AlterColumn<string>(
                name: "building_status",
                table: "building",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Available");

            migrationBuilder.CreateIndex(
                name: "IX_zone_floor_id_zone_code",
                table: "zone",
                columns: new[] { "floor_id", "zone_code" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_zone_capacity",
                table: "zone",
                sql: "capacity >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_BookingId",
                table: "ParkingSessions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_BuildingId",
                table: "ParkingSessions",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_InStaffId",
                table: "ParkingSessions",
                column: "InStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_MonthlySubscriptionId",
                table: "ParkingSessions",
                column: "MonthlySubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_OutStaffId",
                table: "ParkingSessions",
                column: "OutStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_ZoneId",
                table: "ParkingSessions",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_account_InStaffId",
                table: "ParkingSessions",
                column: "InStaffId",
                principalTable: "account",
                principalColumn: "account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_account_OutStaffId",
                table: "ParkingSessions",
                column: "OutStaffId",
                principalTable: "account",
                principalColumn: "account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_booking_BookingId",
                table: "ParkingSessions",
                column: "BookingId",
                principalTable: "booking",
                principalColumn: "booking_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_building_BuildingId",
                table: "ParkingSessions",
                column: "BuildingId",
                principalTable: "building",
                principalColumn: "building_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_card_CardId",
                table: "ParkingSessions",
                column: "CardId",
                principalTable: "card",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_monthly_subscription_MonthlySubscriptionId",
                table: "ParkingSessions",
                column: "MonthlySubscriptionId",
                principalTable: "monthly_subscription",
                principalColumn: "monthly_subscription_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_vehicle_VehicleId",
                table: "ParkingSessions",
                column: "VehicleId",
                principalTable: "vehicle",
                principalColumn: "vehicle_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_zone_ZoneId",
                table: "ParkingSessions",
                column: "ZoneId",
                principalTable: "zone",
                principalColumn: "zone_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_account_InStaffId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_account_OutStaffId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_booking_BookingId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_building_BuildingId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_card_CardId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_monthly_subscription_MonthlySubscriptionId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_vehicle_VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_zone_ZoneId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_zone_floor_id_zone_code",
                table: "zone");

            migrationBuilder.DropCheckConstraint(
                name: "CK_zone_capacity",
                table: "zone");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_BookingId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_BuildingId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_InStaffId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_MonthlySubscriptionId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_OutStaffId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_ZoneId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "zone_access_type",
                table: "zone");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "InStaffId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "LicensePlateIn",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "LicensePlateOut",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "MonthlySubscriptionId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "OutStaffId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "ParkingSessions");

            migrationBuilder.RenameColumn(
                name: "zone_code",
                table: "zone",
                newName: "Code");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "zone",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CardId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "floor_status",
                table: "floor",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Available",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<string>(
                name: "building_status",
                table: "building",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Available",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_zone_floor_id",
                table: "zone",
                column: "floor_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_card_CardId",
                table: "ParkingSessions",
                column: "CardId",
                principalTable: "card",
                principalColumn: "card_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_vehicle_VehicleId",
                table: "ParkingSessions",
                column: "VehicleId",
                principalTable: "vehicle",
                principalColumn: "vehicle_id");
        }
    }
}
