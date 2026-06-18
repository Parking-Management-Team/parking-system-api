using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTypeToRevenueStatistic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "vehicle_type_id",
                table: "revenue_statistic",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_revenue_statistic_vehicle_type_id",
                table: "revenue_statistic",
                column: "vehicle_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_revenue_statistic_vehicle_type_vehicle_type_id",
                table: "revenue_statistic",
                column: "vehicle_type_id",
                principalTable: "vehicle_type",
                principalColumn: "vehicle_type_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_revenue_statistic_vehicle_type_vehicle_type_id",
                table: "revenue_statistic");

            migrationBuilder.DropIndex(
                name: "IX_revenue_statistic_vehicle_type_id",
                table: "revenue_statistic");

            migrationBuilder.DropColumn(
                name: "vehicle_type_id",
                table: "revenue_statistic");
        }
    }
}
