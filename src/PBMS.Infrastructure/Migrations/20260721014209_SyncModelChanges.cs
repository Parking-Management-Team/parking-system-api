using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "permission");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    permission_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    permission_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permission_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permission_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission", x => x.permission_id);
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

            migrationBuilder.CreateIndex(
                name: "IX_permission_permission_code",
                table: "permission",
                column: "permission_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permission_permission_id",
                table: "role_permission",
                column: "permission_id");
        }
    }
}
