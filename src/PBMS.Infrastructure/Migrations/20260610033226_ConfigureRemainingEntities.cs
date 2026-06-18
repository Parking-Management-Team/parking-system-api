using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureRemainingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_parking_slot_VehicleTypes_vehicle_type_id",
                table: "parking_slot");

            migrationBuilder.DropForeignKey(
                name: "FK_zone_VehicleTypes_vehicle_type_id",
                table: "zone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleTypes",
                table: "VehicleTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vehicles",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PricingWindows",
                table: "PricingWindows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PricingPolicies",
                table: "PricingPolicies");

            migrationBuilder.RenameTable(
                name: "VehicleTypes",
                newName: "vehicle_type");

            migrationBuilder.RenameTable(
                name: "Vehicles",
                newName: "vehicle");

            migrationBuilder.RenameTable(
                name: "PricingWindows",
                newName: "pricing_window");

            migrationBuilder.RenameTable(
                name: "PricingPolicies",
                newName: "pricing_policy");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "vehicle_type",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "vehicle_type",
                newName: "vehicle_type_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "vehicle",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "vehicle",
                newName: "vehicle_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "pricing_window",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "pricing_window",
                newName: "pricing_window_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "pricing_policy",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "pricing_policy",
                newName: "pricing_policy_id");

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "ParkingSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "vehicle_type",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "vehicle_type",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type_name",
                table: "vehicle_type",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Cập nhật dữ liệu tạm cho các bản ghi có sẵn để tránh trùng UNIQUE index
            migrationBuilder.Sql("UPDATE vehicle_type SET type_name = 'Motorcycle' WHERE vehicle_type_id = 1;");
            migrationBuilder.Sql("UPDATE vehicle_type SET type_name = 'Car' WHERE vehicle_type_id = 2;");
            migrationBuilder.Sql("UPDATE vehicle_type SET type_name = 'VehicleType_' || vehicle_type_id WHERE type_name = '';");

            migrationBuilder.AddColumn<string>(
                name: "vehicle_type_status",
                table: "vehicle_type",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "vehicle",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "account_id",
                table: "vehicle",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "license_plate",
                table: "vehicle",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "registered_day",
                table: "vehicle",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vehicle_status",
                table: "vehicle",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<int>(
                name: "vehicle_type_id",
                table: "vehicle",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "pricing_window",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "base_duration_minutes",
                table: "pricing_window",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "base_price",
                table: "pricing_window",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "end_time",
                table: "pricing_window",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "grace_period_minutes",
                table: "pricing_window",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "increment_block_minutes",
                table: "pricing_window",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "increment_price",
                table: "pricing_window",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "pricing_policy_id",
                table: "pricing_window",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "start_time",
                table: "pricing_window",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<decimal>(
                name: "window_cap",
                table: "pricing_window",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "window_name",
                table: "pricing_window",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "pricing_policy",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "effective_end",
                table: "pricing_policy",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "effective_start",
                table: "pricing_policy",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "policy_name",
                table: "pricing_policy",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "pricing_policy_status",
                table: "pricing_policy",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<int>(
                name: "vehicle_type_id",
                table: "pricing_policy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle_type",
                table: "vehicle_type",
                column: "vehicle_type_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicle",
                table: "vehicle",
                column: "vehicle_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pricing_window",
                table: "pricing_window",
                column: "pricing_window_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pricing_policy",
                table: "pricing_policy",
                column: "pricing_policy_id");

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    audit_log_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<int>(type: "integer", nullable: true),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    target_table = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    target_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.audit_log_id);
                    table.ForeignKey(
                        name: "FK_audit_log_account_account_id",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "booking",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_type_id = table.Column<int>(type: "integer", nullable: false),
                    building_id = table.Column<int>(type: "integer", nullable: false),
                    planned_checkin_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    planned_checkout_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deposit_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    booking_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    payment_deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    checkin_grace_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancel_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_booking_account_account_id",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_building_building_id",
                        column: x => x.building_id,
                        principalTable: "building",
                        principalColumn: "building_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_vehicle_type_vehicle_type_id",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_type",
                        principalColumn: "vehicle_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicle",
                        principalColumn: "vehicle_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "incident_type",
                columns: table => new
                {
                    incident_type_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    incident_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    incident_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    default_penalty_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_type", x => x.incident_type_id);
                });

            migrationBuilder.CreateTable(
                name: "monthly_subscription",
                columns: table => new
                {
                    monthly_subscription_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_card_id = table.Column<int>(type: "integer", nullable: true),
                    assigned_slot_id = table.Column<int>(type: "integer", nullable: true),
                    building_id = table.Column<int>(type: "integer", nullable: false),
                    monthly_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    activated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    monthly_subscription_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthly_subscription", x => x.monthly_subscription_id);
                    table.ForeignKey(
                        name: "FK_monthly_subscription_account_account_id",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_monthly_subscription_building_building_id",
                        column: x => x.building_id,
                        principalTable: "building",
                        principalColumn: "building_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_monthly_subscription_card_assigned_card_id",
                        column: x => x.assigned_card_id,
                        principalTable: "card",
                        principalColumn: "card_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_monthly_subscription_parking_slot_assigned_slot_id",
                        column: x => x.assigned_slot_id,
                        principalTable: "parking_slot",
                        principalColumn: "slot_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_monthly_subscription_vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicle",
                        principalColumn: "vehicle_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_notification_account_account_id",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    permission_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    permission_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permission_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    permission_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission", x => x.permission_id);
                });

            migrationBuilder.CreateTable(
                name: "revenue_statistic",
                columns: table => new
                {
                    statistic_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    building_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_revenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0.00m),
                    total_bookings = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_sessions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_subscriptions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revenue_statistic", x => x.statistic_id);
                    table.ForeignKey(
                        name: "FK_revenue_statistic_building_building_id",
                        column: x => x.building_id,
                        principalTable: "building",
                        principalColumn: "building_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "incident",
                columns: table => new
                {
                    incident_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_id = table.Column<int>(type: "integer", nullable: false),
                    incident_type_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    penalty_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    incident_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Reported"),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident", x => x.incident_id);
                    table.ForeignKey(
                        name: "FK_incident_ParkingSessions_session_id",
                        column: x => x.session_id,
                        principalTable: "ParkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_incident_incident_type_incident_type_id",
                        column: x => x.incident_type_id,
                        principalTable: "incident_type",
                        principalColumn: "incident_type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    session_id = table.Column<int>(type: "integer", nullable: true),
                    booking_id = table.Column<int>(type: "integer", nullable: true),
                    monthly_subscription_id = table.Column<int>(type: "integer", nullable: true),
                    pricing_policy_id = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    payment_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.payment_id);
                    table.CheckConstraint("CK_Payment_Source", "session_id IS NOT NULL OR booking_id IS NOT NULL OR monthly_subscription_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_payment_ParkingSessions_session_id",
                        column: x => x.session_id,
                        principalTable: "ParkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "booking",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_monthly_subscription_monthly_subscription_id",
                        column: x => x.monthly_subscription_id,
                        principalTable: "monthly_subscription",
                        principalColumn: "monthly_subscription_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_pricing_policy_pricing_policy_id",
                        column: x => x.pricing_policy_id,
                        principalTable: "pricing_policy",
                        principalColumn: "pricing_policy_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    permission_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permission", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "FK_role_permission_permission_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "permission_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permission_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blacklist",
                columns: table => new
                {
                    blacklist_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vehicle_id = table.Column<int>(type: "integer", nullable: true),
                    card_id = table.Column<int>(type: "integer", nullable: true),
                    incident_id = table.Column<int>(type: "integer", nullable: true),
                    reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blacklist", x => x.blacklist_id);
                    table.CheckConstraint("CK_Blacklist_Source", "vehicle_id IS NOT NULL OR card_id IS NOT NULL OR incident_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_blacklist_card_card_id",
                        column: x => x.card_id,
                        principalTable: "card",
                        principalColumn: "card_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_blacklist_incident_incident_id",
                        column: x => x.incident_id,
                        principalTable: "incident",
                        principalColumn: "incident_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_blacklist_vehicle_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicle",
                        principalColumn: "vehicle_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "revenue_statistic_payment",
                columns: table => new
                {
                    statistic_id = table.Column<int>(type: "integer", nullable: false),
                    payment_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revenue_statistic_payment", x => new { x.statistic_id, x.payment_id });
                    table.ForeignKey(
                        name: "FK_revenue_statistic_payment_payment_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payment",
                        principalColumn: "payment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_revenue_statistic_payment_revenue_statistic_statistic_id",
                        column: x => x.statistic_id,
                        principalTable: "revenue_statistic",
                        principalColumn: "statistic_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_VehicleId",
                table: "ParkingSessions",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_type_type_name",
                table: "vehicle_type",
                column: "type_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_account_id",
                table: "vehicle",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_license_plate",
                table: "vehicle",
                column: "license_plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_vehicle_type_id",
                table: "vehicle",
                column: "vehicle_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_window_pricing_policy_id",
                table: "pricing_window",
                column: "pricing_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_policy_vehicle_type_id",
                table: "pricing_policy",
                column: "vehicle_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_account_id",
                table: "audit_log",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_blacklist_card_id",
                table: "blacklist",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "IX_blacklist_incident_id",
                table: "blacklist",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_blacklist_vehicle_id",
                table: "blacklist",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_account_id",
                table: "booking",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_building_id",
                table: "booking",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_vehicle_id",
                table: "booking",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_vehicle_type_id",
                table: "booking",
                column: "vehicle_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_incident_type_id",
                table: "incident",
                column: "incident_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_session_id",
                table: "incident",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_type_incident_code",
                table: "incident_type",
                column: "incident_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_subscription_account_id",
                table: "monthly_subscription",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_subscription_assigned_card_id",
                table: "monthly_subscription",
                column: "assigned_card_id",
                unique: true,
                filter: "assigned_card_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_subscription_assigned_slot_id",
                table: "monthly_subscription",
                column: "assigned_slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_subscription_building_id",
                table: "monthly_subscription",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_subscription_vehicle_id",
                table: "monthly_subscription",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_account_id",
                table: "notification",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_booking_id",
                table: "payment",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_monthly_subscription_id",
                table: "payment",
                column: "monthly_subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_pricing_policy_id",
                table: "payment",
                column: "pricing_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_session_id",
                table: "payment",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_permission_permission_code",
                table: "permission",
                column: "permission_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_revenue_statistic_building_id",
                table: "revenue_statistic",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_revenue_statistic_payment_payment_id",
                table: "revenue_statistic_payment",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permission_permission_id",
                table: "role_permission",
                column: "permission_id");

            migrationBuilder.AddForeignKey(
                name: "FK_parking_slot_vehicle_type_vehicle_type_id",
                table: "parking_slot",
                column: "vehicle_type_id",
                principalTable: "vehicle_type",
                principalColumn: "vehicle_type_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_vehicle_VehicleId",
                table: "ParkingSessions",
                column: "VehicleId",
                principalTable: "vehicle",
                principalColumn: "vehicle_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pricing_policy_vehicle_type_vehicle_type_id",
                table: "pricing_policy",
                column: "vehicle_type_id",
                principalTable: "vehicle_type",
                principalColumn: "vehicle_type_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_pricing_window_pricing_policy_pricing_policy_id",
                table: "pricing_window",
                column: "pricing_policy_id",
                principalTable: "pricing_policy",
                principalColumn: "pricing_policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_account_account_id",
                table: "vehicle",
                column: "account_id",
                principalTable: "account",
                principalColumn: "account_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_vehicle_type_vehicle_type_id",
                table: "vehicle",
                column: "vehicle_type_id",
                principalTable: "vehicle_type",
                principalColumn: "vehicle_type_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_zone_vehicle_type_vehicle_type_id",
                table: "zone",
                column: "vehicle_type_id",
                principalTable: "vehicle_type",
                principalColumn: "vehicle_type_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_parking_slot_vehicle_type_vehicle_type_id",
                table: "parking_slot");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_vehicle_VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_pricing_policy_vehicle_type_vehicle_type_id",
                table: "pricing_policy");

            migrationBuilder.DropForeignKey(
                name: "FK_pricing_window_pricing_policy_pricing_policy_id",
                table: "pricing_window");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_account_account_id",
                table: "vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_vehicle_type_vehicle_type_id",
                table: "vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_zone_vehicle_type_vehicle_type_id",
                table: "zone");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "blacklist");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "revenue_statistic_payment");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "incident");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "revenue_statistic");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "incident_type");

            migrationBuilder.DropTable(
                name: "booking");

            migrationBuilder.DropTable(
                name: "monthly_subscription");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle_type",
                table: "vehicle_type");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_type_type_name",
                table: "vehicle_type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicle",
                table: "vehicle");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_account_id",
                table: "vehicle");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_license_plate",
                table: "vehicle");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_vehicle_type_id",
                table: "vehicle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pricing_window",
                table: "pricing_window");

            migrationBuilder.DropIndex(
                name: "IX_pricing_window_pricing_policy_id",
                table: "pricing_window");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pricing_policy",
                table: "pricing_policy");

            migrationBuilder.DropIndex(
                name: "IX_pricing_policy_vehicle_type_id",
                table: "pricing_policy");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "description",
                table: "vehicle_type");

            migrationBuilder.DropColumn(
                name: "type_name",
                table: "vehicle_type");

            migrationBuilder.DropColumn(
                name: "vehicle_type_status",
                table: "vehicle_type");

            migrationBuilder.DropColumn(
                name: "account_id",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "license_plate",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "registered_day",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "vehicle_status",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "vehicle_type_id",
                table: "vehicle");

            migrationBuilder.DropColumn(
                name: "base_duration_minutes",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "base_price",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "end_time",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "grace_period_minutes",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "increment_block_minutes",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "increment_price",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "pricing_policy_id",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "start_time",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "window_cap",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "window_name",
                table: "pricing_window");

            migrationBuilder.DropColumn(
                name: "effective_end",
                table: "pricing_policy");

            migrationBuilder.DropColumn(
                name: "effective_start",
                table: "pricing_policy");

            migrationBuilder.DropColumn(
                name: "policy_name",
                table: "pricing_policy");

            migrationBuilder.DropColumn(
                name: "pricing_policy_status",
                table: "pricing_policy");

            migrationBuilder.DropColumn(
                name: "vehicle_type_id",
                table: "pricing_policy");

            migrationBuilder.RenameTable(
                name: "vehicle_type",
                newName: "VehicleTypes");

            migrationBuilder.RenameTable(
                name: "vehicle",
                newName: "Vehicles");

            migrationBuilder.RenameTable(
                name: "pricing_window",
                newName: "PricingWindows");

            migrationBuilder.RenameTable(
                name: "pricing_policy",
                newName: "PricingPolicies");

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
                name: "created_at",
                table: "PricingWindows",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "pricing_window_id",
                table: "PricingWindows",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "PricingPolicies",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "pricing_policy_id",
                table: "PricingPolicies",
                newName: "Id");

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

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PricingWindows",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PricingPolicies",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleTypes",
                table: "VehicleTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vehicles",
                table: "Vehicles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PricingWindows",
                table: "PricingWindows",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PricingPolicies",
                table: "PricingPolicies",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_parking_slot_VehicleTypes_vehicle_type_id",
                table: "parking_slot",
                column: "vehicle_type_id",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
