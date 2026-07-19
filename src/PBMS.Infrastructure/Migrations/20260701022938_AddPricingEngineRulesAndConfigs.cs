using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingEngineRulesAndConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pricing_calculation_log",
                columns: table => new
                {
                    pricing_calculation_log_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<int>(type: "integer", nullable: true),
                    parking_session_id = table.Column<int>(type: "integer", nullable: true),
                    vehicle_type_id = table.Column<int>(type: "integer", nullable: false),
                    check_in_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    check_out_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    matched_policy_id = table.Column<int>(type: "integer", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    calculation_details = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_calculation_log", x => x.pricing_calculation_log_id);
                    table.ForeignKey(
                        name: "FK_pricing_calculation_log_pricing_policy_matched_policy_id",
                        column: x => x.matched_policy_id,
                        principalTable: "pricing_policy",
                        principalColumn: "pricing_policy_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pricing_calculation_log_vehicle_type_vehicle_type_id",
                        column: x => x.vehicle_type_id,
                        principalTable: "vehicle_type",
                        principalColumn: "vehicle_type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pricing_rule",
                columns: table => new
                {
                    pricing_rule_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pricing_policy_id = table.Column<int>(type: "integer", nullable: false),
                    rule_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    execution_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_rule", x => x.pricing_rule_id);
                    table.ForeignKey(
                        name: "FK_pricing_rule_pricing_policy_pricing_policy_id",
                        column: x => x.pricing_policy_id,
                        principalTable: "pricing_policy",
                        principalColumn: "pricing_policy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "base_pricing_rule_config",
                columns: table => new
                {
                    base_pricing_rule_config_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pricing_rule_id = table.Column<int>(type: "integer", nullable: false),
                    base_duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    base_price_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_base_pricing_rule_config", x => x.base_pricing_rule_config_id);
                    table.ForeignKey(
                        name: "FK_base_pricing_rule_config_pricing_rule_pricing_rule_id",
                        column: x => x.pricing_rule_id,
                        principalTable: "pricing_rule",
                        principalColumn: "pricing_rule_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "daily_cap_rule_config",
                columns: table => new
                {
                    daily_cap_rule_config_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pricing_rule_id = table.Column<int>(type: "integer", nullable: false),
                    maximum_daily_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_cap_rule_config", x => x.daily_cap_rule_config_id);
                    table.ForeignKey(
                        name: "FK_daily_cap_rule_config_pricing_rule_pricing_rule_id",
                        column: x => x.pricing_rule_id,
                        principalTable: "pricing_rule",
                        principalColumn: "pricing_rule_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grace_period_rule_config",
                columns: table => new
                {
                    grace_period_rule_config_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pricing_rule_id = table.Column<int>(type: "integer", nullable: false),
                    grace_period_minutes = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grace_period_rule_config", x => x.grace_period_rule_config_id);
                    table.ForeignKey(
                        name: "FK_grace_period_rule_config_pricing_rule_pricing_rule_id",
                        column: x => x.pricing_rule_id,
                        principalTable: "pricing_rule",
                        principalColumn: "pricing_rule_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "increment_pricing_rule_config",
                columns: table => new
                {
                    increment_pricing_rule_config_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pricing_rule_id = table.Column<int>(type: "integer", nullable: false),
                    increment_interval_minutes = table.Column<int>(type: "integer", nullable: false),
                    increment_price_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    threshold_percentage = table.Column<int>(type: "integer", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_increment_pricing_rule_config", x => x.increment_pricing_rule_config_id);
                    table.ForeignKey(
                        name: "FK_increment_pricing_rule_config_pricing_rule_pricing_rule_id",
                        column: x => x.pricing_rule_id,
                        principalTable: "pricing_rule",
                        principalColumn: "pricing_rule_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_base_pricing_rule_config_pricing_rule_id",
                table: "base_pricing_rule_config",
                column: "pricing_rule_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_daily_cap_rule_config_pricing_rule_id",
                table: "daily_cap_rule_config",
                column: "pricing_rule_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grace_period_rule_config_pricing_rule_id",
                table: "grace_period_rule_config",
                column: "pricing_rule_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_increment_pricing_rule_config_pricing_rule_id",
                table: "increment_pricing_rule_config",
                column: "pricing_rule_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pricing_calculation_log_booking_id",
                table: "pricing_calculation_log",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_calculation_log_created_at",
                table: "pricing_calculation_log",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_calculation_log_matched_policy_id",
                table: "pricing_calculation_log",
                column: "matched_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_calculation_log_parking_session_id",
                table: "pricing_calculation_log",
                column: "parking_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_calculation_log_vehicle_type_id",
                table: "pricing_calculation_log",
                column: "vehicle_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_rule_pricing_policy_id",
                table: "pricing_rule",
                column: "pricing_policy_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "base_pricing_rule_config");

            migrationBuilder.DropTable(
                name: "daily_cap_rule_config");

            migrationBuilder.DropTable(
                name: "grace_period_rule_config");

            migrationBuilder.DropTable(
                name: "increment_pricing_rule_config");

            migrationBuilder.DropTable(
                name: "pricing_calculation_log");

            migrationBuilder.DropTable(
                name: "pricing_rule");
        }
    }
}
