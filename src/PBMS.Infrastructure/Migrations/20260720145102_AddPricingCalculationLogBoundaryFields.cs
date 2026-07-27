using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingCalculationLogBoundaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "calculation_purpose",
                table: "pricing_calculation_log",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "pricing_calculation_log",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_id",
                table: "pricing_calculation_log",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pricing_calculation_log_idempotency_key",
                table: "pricing_calculation_log",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_calculation_log_payment_id",
                table: "pricing_calculation_log",
                column: "payment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pricing_calculation_log_payment_payment_id",
                table: "pricing_calculation_log",
                column: "payment_id",
                principalTable: "payment",
                principalColumn: "payment_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pricing_calculation_log_payment_payment_id",
                table: "pricing_calculation_log");

            migrationBuilder.DropIndex(
                name: "IX_pricing_calculation_log_idempotency_key",
                table: "pricing_calculation_log");

            migrationBuilder.DropIndex(
                name: "IX_pricing_calculation_log_payment_id",
                table: "pricing_calculation_log");

            migrationBuilder.DropColumn(
                name: "calculation_purpose",
                table: "pricing_calculation_log");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "pricing_calculation_log");

            migrationBuilder.DropColumn(
                name: "payment_id",
                table: "pricing_calculation_log");
        }
    }
}
