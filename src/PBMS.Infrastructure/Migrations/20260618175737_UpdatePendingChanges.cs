using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_zone_capacity",
                table: "zone");

            migrationBuilder.RenameColumn(
                name: "zone_access_type",
                table: "zone",
                newName: "AccessType");

            // 1. Cập nhật các giá trị chuỗi thành dạng số trước khi ép kiểu
            migrationBuilder.Sql("UPDATE zone SET \"AccessType\" = '0' WHERE \"AccessType\" = 'General';");
            migrationBuilder.Sql("UPDATE zone SET \"AccessType\" = '1' WHERE \"AccessType\" = 'Reserved';");
            migrationBuilder.Sql("UPDATE zone SET \"AccessType\" = '0' WHERE \"AccessType\" NOT IN ('0', '1');");

            // 2. Xóa default value cũ 'General' để tránh lỗi cast default value
            migrationBuilder.Sql("ALTER TABLE zone ALTER COLUMN \"AccessType\" DROP DEFAULT;");

            // 3. Ép kiểu cột sang integer
            migrationBuilder.Sql("ALTER TABLE zone ALTER COLUMN \"AccessType\" TYPE integer USING \"AccessType\"::integer;");

            // 4. Cài đặt lại default value mới là 0 (General)
            migrationBuilder.Sql("ALTER TABLE zone ALTER COLUMN \"AccessType\" SET DEFAULT 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddCheckConstraint(
                name: "CK_zone_capacity",
                table: "zone",
                sql: "capacity >= 0");
        }
    }
}
