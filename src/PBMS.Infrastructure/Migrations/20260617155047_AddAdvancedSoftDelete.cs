using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_account_email",
                table: "account");

            migrationBuilder.DropIndex(
                name: "IX_account_username",
                table: "account");

            migrationBuilder.AlterColumn<string>(
                name: "incident_status",
                table: "incident",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Open",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Reported");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "incident",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "deleted_by",
                table: "incident",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "incident",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "blacklist",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "deleted_by",
                table: "blacklist",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "blacklist",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "account",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "deleted_by",
                table: "account",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "account",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_account_email",
                table: "account",
                column: "email",
                unique: true,
                filter: "is_deleted = false AND email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_account_username",
                table: "account",
                column: "username",
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_account_email",
                table: "account");

            migrationBuilder.DropIndex(
                name: "ix_account_username",
                table: "account");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "incident");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "incident");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "incident");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "blacklist");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "blacklist");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "blacklist");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "account");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "account");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "account");

            migrationBuilder.AlterColumn<string>(
                name: "incident_status",
                table: "incident",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Reported",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Open");

            migrationBuilder.CreateIndex(
                name: "IX_account_email",
                table: "account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_username",
                table: "account",
                column: "username",
                unique: true);
        }
    }
}
