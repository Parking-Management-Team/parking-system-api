using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImplementCheckIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE IF EXISTS parking_slot DROP CONSTRAINT IF EXISTS "FK_parking_slot_VehicleTypes_vehicle_type_id";
                ALTER TABLE IF EXISTS "ParkingSessions" DROP CONSTRAINT IF EXISTS "FK_ParkingSessions_card_CardId";
                ALTER TABLE IF EXISTS "ParkingSessions" DROP CONSTRAINT IF EXISTS "FK_ParkingSessions_parking_slot_ParkingSlotId";
                ALTER TABLE IF EXISTS zone DROP CONSTRAINT IF EXISTS "FK_zone_VehicleTypes_vehicle_type_id";

                DO $$
                BEGIN
                    IF to_regclass('public."VehicleTypes"') IS NOT NULL AND to_regclass('public.vehicle_type') IS NULL THEN
                        ALTER TABLE "VehicleTypes" RENAME TO vehicle_type;
                    END IF;

                    IF to_regclass('public."Vehicles"') IS NOT NULL AND to_regclass('public.vehicle') IS NULL THEN
                        ALTER TABLE "Vehicles" RENAME TO vehicle;
                    END IF;

                    IF to_regclass('public."ParkingSessions"') IS NOT NULL AND to_regclass('public.parking_session') IS NULL THEN
                        ALTER TABLE "ParkingSessions" RENAME TO parking_session;
                    END IF;
                END $$;

                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'vehicle_type' AND column_name = 'Id') THEN
                        ALTER TABLE vehicle_type RENAME COLUMN "Id" TO vehicle_type_id;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'vehicle_type' AND column_name = 'CreatedAt') THEN
                        ALTER TABLE vehicle_type RENAME COLUMN "CreatedAt" TO created_at;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'vehicle' AND column_name = 'Id') THEN
                        ALTER TABLE vehicle RENAME COLUMN "Id" TO vehicle_id;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'vehicle' AND column_name = 'CreatedAt') THEN
                        ALTER TABLE vehicle RENAME COLUMN "CreatedAt" TO created_at;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parking_session' AND column_name = 'Id') THEN
                        ALTER TABLE parking_session RENAME COLUMN "Id" TO parking_session_id;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parking_session' AND column_name = 'CardId') THEN
                        ALTER TABLE parking_session RENAME COLUMN "CardId" TO card_id;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parking_session' AND column_name = 'ParkingSlotId') THEN
                        ALTER TABLE parking_session RENAME COLUMN "ParkingSlotId" TO slot_id;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parking_session' AND column_name = 'SessionStatus') THEN
                        ALTER TABLE parking_session RENAME COLUMN "SessionStatus" TO session_status;
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'parking_session' AND column_name = 'CreatedAt') THEN
                        ALTER TABLE parking_session RENAME COLUMN "CreatedAt" TO created_at;
                    END IF;
                END $$;

                ALTER TABLE vehicle_type ADD COLUMN IF NOT EXISTS vehicle_type_code character varying(20);
                ALTER TABLE vehicle_type ADD COLUMN IF NOT EXISTS vehicle_type_name character varying(50);
                UPDATE vehicle_type
                SET vehicle_type_code = 'TYPE-' || vehicle_type_id
                WHERE vehicle_type_code IS NULL OR vehicle_type_code = '';
                UPDATE vehicle_type
                SET vehicle_type_name = 'Vehicle Type ' || vehicle_type_id
                WHERE vehicle_type_name IS NULL OR vehicle_type_name = '';
                ALTER TABLE vehicle_type ALTER COLUMN vehicle_type_code SET NOT NULL;
                ALTER TABLE vehicle_type ALTER COLUMN vehicle_type_name SET NOT NULL;
                ALTER TABLE vehicle_type ALTER COLUMN created_at SET DEFAULT CURRENT_TIMESTAMP;

                INSERT INTO vehicle_type (vehicle_type_code, vehicle_type_name, created_at)
                SELECT 'UNKNOWN', 'Unknown', CURRENT_TIMESTAMP
                WHERE NOT EXISTS (SELECT 1 FROM vehicle_type);

                ALTER TABLE vehicle ADD COLUMN IF NOT EXISTS license_plate character varying(20);
                ALTER TABLE vehicle ADD COLUMN IF NOT EXISTS vehicle_type_id integer;
                UPDATE vehicle
                SET license_plate = 'UNKNOWN-' || vehicle_id
                WHERE license_plate IS NULL OR license_plate = '';
                UPDATE vehicle
                SET vehicle_type_id = (SELECT vehicle_type_id FROM vehicle_type ORDER BY vehicle_type_id LIMIT 1)
                WHERE vehicle_type_id IS NULL OR vehicle_type_id = 0;
                ALTER TABLE vehicle ALTER COLUMN license_plate SET NOT NULL;
                ALTER TABLE vehicle ALTER COLUMN vehicle_type_id SET NOT NULL;
                ALTER TABLE vehicle ALTER COLUMN created_at SET DEFAULT CURRENT_TIMESTAMP;

                ALTER TABLE parking_session ADD COLUMN IF NOT EXISTS checkin_time timestamp with time zone;
                ALTER TABLE parking_session ADD COLUMN IF NOT EXISTS checkout_time timestamp with time zone;
                ALTER TABLE parking_session ADD COLUMN IF NOT EXISTS in_staff_id integer;
                ALTER TABLE parking_session ADD COLUMN IF NOT EXISTS out_staff_id integer;
                ALTER TABLE parking_session ADD COLUMN IF NOT EXISTS vehicle_id integer;
                ALTER TABLE parking_session ADD COLUMN IF NOT EXISTS zone_id integer;
                UPDATE parking_session
                SET checkin_time = created_at
                WHERE checkin_time IS NULL;
                ALTER TABLE parking_session ALTER COLUMN checkin_time SET NOT NULL;
                ALTER TABLE parking_session ALTER COLUMN session_status TYPE character varying(20);
                ALTER TABLE parking_session ALTER COLUMN session_status SET DEFAULT 'Active';
                ALTER TABLE parking_session ALTER COLUMN created_at SET DEFAULT CURRENT_TIMESTAMP;

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_vehicle_type_code" ON vehicle_type (vehicle_type_code);
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_vehicle_license_plate" ON vehicle (license_plate);
                CREATE INDEX IF NOT EXISTS "IX_vehicle_vehicle_type_id" ON vehicle (vehicle_type_id);
                CREATE INDEX IF NOT EXISTS "IX_parking_session_vehicle_id" ON parking_session (vehicle_id);
                CREATE INDEX IF NOT EXISTS "IX_parking_session_zone_id" ON parking_session (zone_id);
                CREATE INDEX IF NOT EXISTS "IX_parking_session_card_id" ON parking_session (card_id);
                CREATE INDEX IF NOT EXISTS "IX_parking_session_slot_id" ON parking_session (slot_id);

                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_parking_slot_vehicle_type_vehicle_type_id') THEN
                        ALTER TABLE parking_slot
                        ADD CONSTRAINT "FK_parking_slot_vehicle_type_vehicle_type_id"
                        FOREIGN KEY (vehicle_type_id) REFERENCES vehicle_type (vehicle_type_id) ON DELETE RESTRICT;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_zone_vehicle_type_vehicle_type_id') THEN
                        ALTER TABLE zone
                        ADD CONSTRAINT "FK_zone_vehicle_type_vehicle_type_id"
                        FOREIGN KEY (vehicle_type_id) REFERENCES vehicle_type (vehicle_type_id) ON DELETE RESTRICT;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_vehicle_vehicle_type_vehicle_type_id') THEN
                        ALTER TABLE vehicle
                        ADD CONSTRAINT "FK_vehicle_vehicle_type_vehicle_type_id"
                        FOREIGN KEY (vehicle_type_id) REFERENCES vehicle_type (vehicle_type_id) ON DELETE RESTRICT;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_parking_session_card_card_id') THEN
                        ALTER TABLE parking_session
                        ADD CONSTRAINT "FK_parking_session_card_card_id"
                        FOREIGN KEY (card_id) REFERENCES card (card_id) ON DELETE RESTRICT NOT VALID;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_parking_session_parking_slot_slot_id') THEN
                        ALTER TABLE parking_session
                        ADD CONSTRAINT "FK_parking_session_parking_slot_slot_id"
                        FOREIGN KEY (slot_id) REFERENCES parking_slot (slot_id) ON DELETE RESTRICT NOT VALID;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_parking_session_vehicle_vehicle_id') THEN
                        ALTER TABLE parking_session
                        ADD CONSTRAINT "FK_parking_session_vehicle_vehicle_id"
                        FOREIGN KEY (vehicle_id) REFERENCES vehicle (vehicle_id) ON DELETE RESTRICT NOT VALID;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_parking_session_zone_zone_id') THEN
                        ALTER TABLE parking_session
                        ADD CONSTRAINT "FK_parking_session_zone_zone_id"
                        FOREIGN KEY (zone_id) REFERENCES zone (zone_id) ON DELETE RESTRICT NOT VALID;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_parking_session_card_card_id",
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
                name: "FK_parking_slot_vehicle_type_vehicle_type_id",
                table: "parking_slot");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_vehicle_type_vehicle_type_id",
                table: "vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_zone_vehicle_type_vehicle_type_id",
                table: "zone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_type",
                table: "vehicle_type");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_type_code",
                table: "vehicle_type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle",
                table: "vehicle");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_license_plate",
                table: "vehicle");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_vehicle_type_id",
                table: "vehicle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_parking_session",
                table: "parking_session");

            migrationBuilder.DropIndex(
                name: "IX_parking_session_vehicle_id",
                table: "parking_session");

            migrationBuilder.DropIndex(
                name: "IX_parking_session_zone_id",
                table: "parking_session");

            migrationBuilder.DropColumn(
                name: "vehicle_type_code",
                table: "vehicle_type");

            migrationBuilder.DropColumn(
                name: "vehicle_type_name",
                table: "vehicle_type");

            migrationBuilder.DropColumn(
                name: "license_plate",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "vehicle_type_id",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "checkin_time",
                table: "parking_session");

            migrationBuilder.DropColumn(
                name: "checkout_time",
                table: "parking_session");

            migrationBuilder.DropColumn(
                name: "in_staff_id",
                table: "parking_session");

            migrationBuilder.DropColumn(
                name: "out_staff_id",
                table: "parking_session");

            migrationBuilder.DropColumn(
                name: "vehicle_id",
                table: "parking_session");

            migrationBuilder.DropColumn(
                name: "zone_id",
                table: "parking_session");

            migrationBuilder.RenameTable(
                name: "vehicle_type",
                newName: "VehicleTypes");

            migrationBuilder.RenameTable(
                name: "vehicle",
                newName: "Vehicles");

            migrationBuilder.RenameTable(
                name: "parking_session",
                newName: "ParkingSessions");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "VehicleTypes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "vehicle_type_id",
                table: "VehicleTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Vehicles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "vehicle_id",
                table: "Vehicles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "slot_id",
                table: "ParkingSessions",
                newName: "ParkingSlotId");

            migrationBuilder.RenameColumn(
                name: "session_status",
                table: "ParkingSessions",
                newName: "SessionStatus");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ParkingSessions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "card_id",
                table: "ParkingSessions",
                newName: "CardId");

            migrationBuilder.RenameColumn(
                name: "parking_session_id",
                table: "ParkingSessions",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_parking_session_slot_id",
                table: "ParkingSessions",
                newName: "IX_ParkingSessions_ParkingSlotId");

            migrationBuilder.RenameIndex(
                name: "IX_parking_session_card_id",
                table: "ParkingSessions",
                newName: "IX_ParkingSessions_CardId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "VehicleTypes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Vehicles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "SessionStatus",
                table: "ParkingSessions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ParkingSessions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<int>(
                name: "CardId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleTypes",
                table: "VehicleTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vehicles",
                table: "Vehicles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSessions",
                table: "ParkingSessions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_parking_slot_VehicleTypes_vehicle_type_id",
                table: "parking_slot",
                column: "vehicle_type_id",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_card_CardId",
                table: "ParkingSessions",
                column: "CardId",
                principalTable: "card",
                principalColumn: "card_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_parking_slot_ParkingSlotId",
                table: "ParkingSessions",
                column: "ParkingSlotId",
                principalTable: "parking_slot",
                principalColumn: "slot_id");

            migrationBuilder.AddForeignKey(
                name: "FK_zone_VehicleTypes_vehicle_type_id",
                table: "zone",
                column: "vehicle_type_id",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
