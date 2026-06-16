using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <summary>
    /// Supabase already has parking_session in the SRS shape; this migration
    /// records that model alignment in EF history without altering live schema.
    /// </summary>
    public partial class AlignParkingSessionWithSrs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
