using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCodeAndFixPaymentConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Payment_Source",
                table: "payment");

            migrationBuilder.AddColumn<long>(
                name: "order_code",
                table: "payment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payment_Source",
                table: "payment",
                sql: "(CASE WHEN session_id IS NULL THEN 0 ELSE 1 END + CASE WHEN booking_id IS NULL THEN 0 ELSE 1 END + CASE WHEN monthly_subscription_id IS NULL THEN 0 ELSE 1 END) = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Payment_Source",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "order_code",
                table: "payment");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payment_Source",
                table: "payment",
                sql: "session_id IS NOT NULL OR booking_id IS NOT NULL OR monthly_subscription_id IS NOT NULL");
        }
    }
}
