using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncVehicleTypeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cột vehicle_type_code đã được tạo trước đó bằng raw SQL ở migration cũ, 
            // nên ta để trống phương thức này để tránh lỗi cột đã tồn tại.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Để trống để đồng bộ
        }
    }
}
