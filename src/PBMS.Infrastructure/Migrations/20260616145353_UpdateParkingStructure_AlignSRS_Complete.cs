using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateParkingStructure_AlignSRS_Complete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_incident_ParkingSessions_session_id",
                table: "incident");

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
                name: "FK_ParkingSessions_parking_slot_ParkingSlotId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_vehicle_VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_zone_ZoneId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_ParkingSessions_session_id",
                table: "payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingSessions",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_BookingId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_CardId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_ParkingSlotId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "ParkingSlotId",
                table: "ParkingSessions");

            migrationBuilder.RenameTable(
                name: "ParkingSessions",
                newName: "parking_session");

            migrationBuilder.RenameColumn(
                name: "ZoneId",
                table: "parking_session",
                newName: "zone_id");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "parking_session",
                newName: "vehicle_id");

            migrationBuilder.RenameColumn(
                name: "SlotId",
                table: "parking_session",
                newName: "slot_id");

            migrationBuilder.RenameColumn(
                name: "SessionStatus",
                table: "parking_session",
                newName: "session_status");

            migrationBuilder.RenameColumn(
                name: "OutStaffId",
                table: "parking_session",
                newName: "out_staff_id");

            migrationBuilder.RenameColumn(
                name: "MonthlySubscriptionId",
                table: "parking_session",
                newName: "monthly_subscription_id");

            migrationBuilder.RenameColumn(
                name: "LicensePlateOut",
                table: "parking_session",
                newName: "license_plate_out");

            migrationBuilder.RenameColumn(
                name: "LicensePlateIn",
                table: "parking_session",
                newName: "license_plate_in");

            migrationBuilder.RenameColumn(
                name: "InStaffId",
                table: "parking_session",
                newName: "in_staff_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "parking_session",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CheckOutTime",
                table: "parking_session",
                newName: "check_out_time");

            migrationBuilder.RenameColumn(
                name: "CheckInTime",
                table: "parking_session",
                newName: "check_in_time");

            migrationBuilder.RenameColumn(
                name: "CardId",
                table: "parking_session",
                newName: "card_id");

            migrationBuilder.RenameColumn(
                name: "BuildingId",
                table: "parking_session",
                newName: "building_id");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "parking_session",
                newName: "booking_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "parking_session",
                newName: "session_id");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSessions_ZoneId",
                table: "parking_session",
                newName: "IX_parking_session_zone_id");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSessions_OutStaffId",
                table: "parking_session",
                newName: "IX_parking_session_out_staff_id");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSessions_MonthlySubscriptionId",
                table: "parking_session",
                newName: "IX_parking_session_monthly_subscription_id");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSessions_InStaffId",
                table: "parking_session",
                newName: "IX_parking_session_in_staff_id");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSessions_BuildingId",
                table: "parking_session",
                newName: "IX_parking_session_building_id");

            migrationBuilder.AlterColumn<string>(
                name: "vehicle_type_status",
                table: "vehicle_type",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVE",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<string>(
                name: "vehicle_status",
                table: "vehicle",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVE",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<string>(
                name: "session_status",
                table: "parking_session",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVE",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "license_plate_out",
                table: "parking_session",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "license_plate_in",
                table: "parking_session",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "parking_session",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_parking_session",
                table: "parking_session",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_parking_session_active_card",
                table: "parking_session",
                column: "card_id",
                unique: true,
                filter: "upper(session_status) = 'ACTIVE'");

            migrationBuilder.CreateIndex(
                name: "IX_parking_session_active_slot",
                table: "parking_session",
                column: "slot_id",
                unique: true,
                filter: "slot_id IS NOT NULL AND upper(session_status) = 'ACTIVE'");

            migrationBuilder.CreateIndex(
                name: "IX_parking_session_active_vehicle",
                table: "parking_session",
                column: "vehicle_id",
                unique: true,
                filter: "upper(session_status) = 'ACTIVE'");

            migrationBuilder.CreateIndex(
                name: "IX_parking_session_booking_id",
                table: "parking_session",
                column: "booking_id",
                unique: true,
                filter: "booking_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_parking_session_source_exclusive",
                table: "parking_session",
                sql: "booking_id IS NULL OR monthly_subscription_id IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_incident_parking_session_session_id",
                table: "incident",
                column: "session_id",
                principalTable: "parking_session",
                principalColumn: "session_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parking_session_account_in_staff_id",
                table: "parking_session",
                column: "in_staff_id",
                principalTable: "account",
                principalColumn: "account_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parking_session_account_out_staff_id",
                table: "parking_session",
                column: "out_staff_id",
                principalTable: "account",
                principalColumn: "account_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parking_session_booking_booking_id",
                table: "parking_session",
                column: "booking_id",
                principalTable: "booking",
                principalColumn: "booking_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parking_session_building_building_id",
                table: "parking_session",
                column: "building_id",
                principalTable: "building",
                principalColumn: "building_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parking_session_card_card_id",
                table: "parking_session",
                column: "card_id",
                principalTable: "card",
                principalColumn: "card_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parking_session_monthly_subscription_monthly_subscription_id",
                table: "parking_session",
                column: "monthly_subscription_id",
                principalTable: "monthly_subscription",
                principalColumn: "monthly_subscription_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parking_session_parking_slot_slot_id",
                table: "parking_session",
                column: "slot_id",
                principalTable: "parking_slot",
                principalColumn: "slot_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parking_session_vehicle_vehicle_id",
                table: "parking_session",
                column: "vehicle_id",
                principalTable: "vehicle",
                principalColumn: "vehicle_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_parking_session_zone_zone_id",
                table: "parking_session",
                column: "zone_id",
                principalTable: "zone",
                principalColumn: "zone_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_parking_session_session_id",
                table: "payment",
                column: "session_id",
                principalTable: "parking_session",
                principalColumn: "session_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_incident_parking_session_session_id",
                table: "incident");

            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_account_in_staff_id",
                table: "parking_session");

            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_account_out_staff_id",
                table: "parking_session");

            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_booking_booking_id",
                table: "parking_session");

            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_building_building_id",
                table: "parking_session");

            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_card_card_id",
                table: "parking_session");

            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_monthly_subscription_monthly_subscription_id",
                table: "parking_session");

            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_parking_slot_slot_id",
                table: "parking_session");

            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_vehicle_vehicle_id",
                table: "parking_session");

            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_zone_zone_id",
                table: "parking_session");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_parking_session_session_id",
                table: "payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_parking_session",
                table: "parking_session");

            migrationBuilder.DropIndex(
                name: "IX_parking_session_active_card",
                table: "parking_session");

            migrationBuilder.DropIndex(
                name: "IX_parking_session_active_slot",
                table: "parking_session");

            migrationBuilder.DropIndex(
                name: "IX_parking_session_active_vehicle",
                table: "parking_session");

            migrationBuilder.DropIndex(
                name: "IX_parking_session_booking_id",
                table: "parking_session");

            migrationBuilder.DropCheckConstraint(
                name: "CK_parking_session_source_exclusive",
                table: "parking_session");

            migrationBuilder.RenameTable(
                name: "parking_session",
                newName: "ParkingSessions");

            migrationBuilder.RenameColumn(
                name: "zone_id",
                table: "ParkingSessions",
                newName: "ZoneId");

            migrationBuilder.RenameColumn(
                name: "vehicle_id",
                table: "ParkingSessions",
                newName: "VehicleId");

            migrationBuilder.RenameColumn(
                name: "slot_id",
                table: "ParkingSessions",
                newName: "SlotId");

            migrationBuilder.RenameColumn(
                name: "session_status",
                table: "ParkingSessions",
                newName: "SessionStatus");

            migrationBuilder.RenameColumn(
                name: "out_staff_id",
                table: "ParkingSessions",
                newName: "OutStaffId");

            migrationBuilder.RenameColumn(
                name: "monthly_subscription_id",
                table: "ParkingSessions",
                newName: "MonthlySubscriptionId");

            migrationBuilder.RenameColumn(
                name: "license_plate_out",
                table: "ParkingSessions",
                newName: "LicensePlateOut");

            migrationBuilder.RenameColumn(
                name: "license_plate_in",
                table: "ParkingSessions",
                newName: "LicensePlateIn");

            migrationBuilder.RenameColumn(
                name: "in_staff_id",
                table: "ParkingSessions",
                newName: "InStaffId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ParkingSessions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "check_out_time",
                table: "ParkingSessions",
                newName: "CheckOutTime");

            migrationBuilder.RenameColumn(
                name: "check_in_time",
                table: "ParkingSessions",
                newName: "CheckInTime");

            migrationBuilder.RenameColumn(
                name: "card_id",
                table: "ParkingSessions",
                newName: "CardId");

            migrationBuilder.RenameColumn(
                name: "building_id",
                table: "ParkingSessions",
                newName: "BuildingId");

            migrationBuilder.RenameColumn(
                name: "booking_id",
                table: "ParkingSessions",
                newName: "BookingId");

            migrationBuilder.RenameColumn(
                name: "session_id",
                table: "ParkingSessions",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_parking_session_zone_id",
                table: "ParkingSessions",
                newName: "IX_ParkingSessions_ZoneId");

            migrationBuilder.RenameIndex(
                name: "IX_parking_session_out_staff_id",
                table: "ParkingSessions",
                newName: "IX_ParkingSessions_OutStaffId");

            migrationBuilder.RenameIndex(
                name: "IX_parking_session_monthly_subscription_id",
                table: "ParkingSessions",
                newName: "IX_ParkingSessions_MonthlySubscriptionId");

            migrationBuilder.RenameIndex(
                name: "IX_parking_session_in_staff_id",
                table: "ParkingSessions",
                newName: "IX_ParkingSessions_InStaffId");

            migrationBuilder.RenameIndex(
                name: "IX_parking_session_building_id",
                table: "ParkingSessions",
                newName: "IX_ParkingSessions_BuildingId");

            migrationBuilder.AlterColumn<string>(
                name: "vehicle_type_status",
                table: "vehicle_type",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "ACTIVE");

            migrationBuilder.AlterColumn<string>(
                name: "vehicle_status",
                table: "vehicle",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "ACTIVE");

            migrationBuilder.AlterColumn<string>(
                name: "SessionStatus",
                table: "ParkingSessions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "ACTIVE");

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlateOut",
                table: "ParkingSessions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlateIn",
                table: "ParkingSessions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ParkingSessions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "ParkingSlotId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSessions",
                table: "ParkingSessions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_BookingId",
                table: "ParkingSessions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_CardId",
                table: "ParkingSessions",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_ParkingSlotId",
                table: "ParkingSessions",
                column: "ParkingSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_VehicleId",
                table: "ParkingSessions",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_incident_ParkingSessions_session_id",
                table: "incident",
                column: "session_id",
                principalTable: "ParkingSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_ParkingSessions_parking_slot_ParkingSlotId",
                table: "ParkingSessions",
                column: "ParkingSlotId",
                principalTable: "parking_slot",
                principalColumn: "slot_id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_payment_ParkingSessions_session_id",
                table: "payment",
                column: "session_id",
                principalTable: "ParkingSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
