using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncVehicleTypeNameMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- 1. Drop the old unique index if it exists
                DROP INDEX IF EXISTS ""IX_vehicle_type_type_name"";
                
                -- 2. Drop the unused 'type_name' column if it exists
                ALTER TABLE vehicle_type DROP COLUMN IF EXISTS type_name;
                
                -- 3. Re-create the unique index on the correct 'vehicle_type_name' column
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_vehicle_type_type_name"" ON vehicle_type (vehicle_type_name);

                -- 4. Drop the unused 'checkin_time' and 'checkout_time' columns from 'parking_session' table
                ALTER TABLE parking_session DROP COLUMN IF EXISTS checkin_time;
                ALTER TABLE parking_session DROP COLUMN IF EXISTS checkout_time;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Để trống để đồng bộ
        }
    }
}
