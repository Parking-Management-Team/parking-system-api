using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "default_penalty_fee",
                table: "incident_type");

            migrationBuilder.RenameColumn(
                name: "AccessType",
                table: "zone",
                newName: "zone_access_type");

            migrationBuilder.AlterColumn<string>(
                name: "zone_access_type",
                table: "zone",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "General",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionPriceConfigId",
                table: "monthly_subscription",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PenaltyConfigId",
                table: "incident",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PenaltyConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentTypeId = table.Column<int>(type: "integer", nullable: false),
                    PenaltyFee = table.Column<decimal>(type: "numeric", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PenaltyConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PenaltyConfigs_incident_type_IncidentTypeId",
                        column: x => x.IncidentTypeId,
                        principalTable: "incident_type",
                        principalColumn: "incident_type_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPriceConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleTypeId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPriceConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPriceConfigs_vehicle_type_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "vehicle_type",
                        principalColumn: "vehicle_type_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_monthly_subscription_SubscriptionPriceConfigId",
                table: "monthly_subscription",
                column: "SubscriptionPriceConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_incident_PenaltyConfigId",
                table: "incident",
                column: "PenaltyConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_PenaltyConfigs_IncidentTypeId",
                table: "PenaltyConfigs",
                column: "IncidentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPriceConfigs_VehicleTypeId",
                table: "SubscriptionPriceConfigs",
                column: "VehicleTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_incident_PenaltyConfigs_PenaltyConfigId",
                table: "incident",
                column: "PenaltyConfigId",
                principalTable: "PenaltyConfigs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_subscription_SubscriptionPriceConfigs_SubscriptionP~",
                table: "monthly_subscription",
                column: "SubscriptionPriceConfigId",
                principalTable: "SubscriptionPriceConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_incident_PenaltyConfigs_PenaltyConfigId",
                table: "incident");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_subscription_SubscriptionPriceConfigs_SubscriptionP~",
                table: "monthly_subscription");

            migrationBuilder.DropTable(
                name: "PenaltyConfigs");

            migrationBuilder.DropTable(
                name: "SubscriptionPriceConfigs");

            migrationBuilder.DropIndex(
                name: "IX_monthly_subscription_SubscriptionPriceConfigId",
                table: "monthly_subscription");

            migrationBuilder.DropIndex(
                name: "IX_incident_PenaltyConfigId",
                table: "incident");

            migrationBuilder.DropColumn(
                name: "SubscriptionPriceConfigId",
                table: "monthly_subscription");

            migrationBuilder.DropColumn(
                name: "PenaltyConfigId",
                table: "incident");

            migrationBuilder.RenameColumn(
                name: "zone_access_type",
                table: "zone",
                newName: "AccessType");

            migrationBuilder.AlterColumn<int>(
                name: "AccessType",
                table: "zone",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "General");

            migrationBuilder.AddColumn<decimal>(
                name: "default_penalty_fee",
                table: "incident_type",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }
    }
}
