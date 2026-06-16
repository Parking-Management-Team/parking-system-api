using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng Vehicle (Phương tiện) sử dụng Fluent API của EF Core.
    /// Định nghĩa tên bảng, khóa chính, ràng buộc độc nhất (license_plate) và các khóa ngoại.
    /// </summary>
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            // 1. Ánh xạ thực thể Vehicle với tên bảng vật lý "vehicle" trong database
            builder.ToTable("vehicle");

            // 2. Chỉ định khóa chính
            builder.HasKey(v => v.Id);

            // 3. Cấu hình cột khóa chính (Id) ánh xạ vào cột "vehicle_id" và tự động sinh tăng dần
            builder.Property(v => v.Id)
                .HasColumnName("vehicle_id")
                .ValueGeneratedOnAdd();

            // 4. Cấu hình khóa ngoại AccountId: có thể null
            builder.Property(v => v.AccountId)
                .HasColumnName("account_id");

            // 5. Cấu hình khóa ngoại VehicleTypeId: bắt buộc
            builder.Property(v => v.VehicleTypeId)
                .HasColumnName("vehicle_type_id")
                .IsRequired();

            // 6. Biển số xe (LicensePlate): tối đa 20 ký tự, bắt buộc nhập
            builder.Property(v => v.LicensePlate)
                .HasColumnName("license_plate")
                .HasMaxLength(20)
                .IsRequired();

            // Tạo chỉ mục độc nhất (Unique Index) để ngăn trùng biển số xe
            builder.HasIndex(v => v.LicensePlate)
                .IsUnique();

            // 7. Ngày đăng ký xe vào hệ thống (RegisteredDay): chỉ lưu ngày (kiểu date trong DB), cho phép null
            builder.Property(v => v.RegisteredDay)
                .HasColumnName("registered_day")
                .HasColumnType("date");

            // 8. Trạng thái xe (VehicleStatus): tối đa 20 ký tự, mặc định "Active", bắt buộc nhập
            builder.Property(v => v.VehicleStatus)
                .HasColumnName("vehicle_status")
                .HasMaxLength(20)
                .HasDefaultValue(Vehicle.StatusActive)
                .IsRequired();

            // 9. Cấu hình cột RowVersion (BaseEntity) để kiểm soát xung đột đồng thời
            builder.Property(v => v.RowVersion)
                .IsRowVersion();

            // 10. Thời điểm tạo bản ghi (CreatedAt - thuộc tính kế thừa từ BaseEntity)
            builder.Property(v => v.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // =======================================================================
            // Cấu hình các mối quan hệ (Relationships)
            // =======================================================================

            // Quan hệ N-1: Nhiều xe thuộc sở hữu của 1 tài khoản (Account)
            builder.HasOne(v => v.Account)
                .WithMany(a => a.Vehicles)
                .HasForeignKey(v => v.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ N-1: Nhiều xe thuộc 1 loại phương tiện (VehicleType)
            builder.HasOne(v => v.VehicleType)
                .WithMany(vt => vt.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
