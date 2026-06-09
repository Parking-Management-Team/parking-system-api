using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "building",
                columns: table => new
                {
                    building_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    building_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    building_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    total_floor = table.Column<int>(type: "integer", nullable: false),
                    building_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building", x => x.building_id);
                });

            migrationBuilder.CreateTable(
                name: "card",
                columns: table => new
                {
                    card_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    card_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rfid_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    card_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    card_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card", x => x.card_id);
                });

            migrationBuilder.CreateTable(
                name: "PricingPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingWindows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingWindows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "floor",
                columns: table => new
                {
                    floor_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    building_id = table.Column<int>(type: "integer", nullable: false),
                    floor_number = table.Column<int>(type: "integer", nullable: false),
                    floor_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    floor_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_floor", x => x.floor_id);
                    table.ForeignKey(
                        name: "FK_floor_building_building_id",
                        column: x => x.building_id,
                        principalTable: "building",
                        principalColumn: "building_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    account_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.account_id);
                    table.ForeignKey(
                        name: "FK_account_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "zone",
                columns: table => new
                {
                    zone_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    floor_id = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vehicle_type_id = table.Column<int>(type: "integer", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zone", x => x.zone_id);
                    table.ForeignKey(
                        name: "FK_zone_VehicleTypes_vehicle_type_id",
                        column: x => x.vehicle_type_id,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_zone_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "floor",
                        principalColumn: "floor_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "parking_slot",
                columns: table => new
                {
                    slot_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zone_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_type_id = table.Column<int>(type: "integer", nullable: false),
                    slot_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    slot_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    slot_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parking_slot", x => x.slot_id);
                    table.ForeignKey(
                        name: "FK_parking_slot_VehicleTypes_vehicle_type_id",
                        column: x => x.vehicle_type_id,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parking_slot_zone_zone_id",
                        column: x => x.zone_id,
                        principalTable: "zone",
                        principalColumn: "zone_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CardId = table.Column<int>(type: "integer", nullable: true),
                    SessionStatus = table.Column<string>(type: "text", nullable: false),
                    ParkingSlotId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_card_CardId",
                        column: x => x.CardId,
                        principalTable: "card",
                        principalColumn: "card_id");
                    table.ForeignKey(
                        name: "FK_ParkingSessions_parking_slot_ParkingSlotId",
                        column: x => x.ParkingSlotId,
                        principalTable: "parking_slot",
                        principalColumn: "slot_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_email",
                table: "account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_role_id",
                table: "account",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_account_username",
                table: "account",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_building_building_code",
                table: "building",
                column: "building_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_card_card_code",
                table: "card",
                column: "card_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_card_rfid_code",
                table: "card",
                column: "rfid_code",
                unique: true,
                filter: "rfid_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_floor_building_id_floor_number",
                table: "floor",
                columns: new[] { "building_id", "floor_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parking_slot_slot_code",
                table: "parking_slot",
                column: "slot_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parking_slot_vehicle_type_id",
                table: "parking_slot",
                column: "vehicle_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_parking_slot_zone_id",
                table: "parking_slot",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_CardId",
                table: "ParkingSessions",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_ParkingSlotId",
                table: "ParkingSessions",
                column: "ParkingSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_role_role_name",
                table: "role",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_zone_floor_id",
                table: "zone",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_zone_vehicle_type_id",
                table: "zone",
                column: "vehicle_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "ParkingSessions");

            migrationBuilder.DropTable(
                name: "PricingPolicies");

            migrationBuilder.DropTable(
                name: "PricingWindows");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "card");

            migrationBuilder.DropTable(
                name: "parking_slot");

            migrationBuilder.DropTable(
                name: "zone");

            migrationBuilder.DropTable(
                name: "VehicleTypes");

            migrationBuilder.DropTable(
                name: "floor");

            migrationBuilder.DropTable(
                name: "building");
        }
    }
}
