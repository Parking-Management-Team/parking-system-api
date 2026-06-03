using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations
{
    /// <summary>
    /// Lớp cấu hình bảng Account (tương đương User) trong Database sử dụng Fluent API của EF Core.
    /// Định nghĩa các ràng buộc dữ liệu, chỉ mục (Index) và mối quan hệ giữa các bảng.
    /// </summary>
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            // 1. Ánh xạ thực thể Account với tên bảng vật lý "account" trong database
            builder.ToTable("account");

            // 2. Định nghĩa khóa chính cho bảng
            builder.HasKey(a => a.Id);

            // 3. Cấu hình cột khóa chính (Id) ánh xạ vào cột "account_id" và tự động sinh tăng dần (Serial/Identity)
            builder.Property(a => a.Id)
                .HasColumnName("account_id")
                .ValueGeneratedOnAdd();

            // 4. Khóa ngoại RoleId ánh xạ vào cột "role_id", bắt buộc phải có dữ liệu (NOT NULL)
            builder.Property(a => a.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            // 5. Tên đăng nhập (Username): tối đa 50 ký tự, bắt buộc phải nhập
            builder.Property(a => a.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            // Tạo chỉ mục độc nhất (Unique Index) để đảm bảo không trùng lặp Username
            builder.HasIndex(a => a.Username)
                .IsUnique();

            // 6. Mật khẩu mã hóa (PasswordHash): tối đa 100 ký tự, bắt buộc phải nhập
            builder.Property(a => a.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(100)
                .IsRequired();

            // 7. Họ và tên (FullName): tối đa 100 ký tự, cho phép null
            builder.Property(a => a.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(100);

            // 8. Thư điện tử (Email): tối đa 100 ký tự, cho phép null
            builder.Property(a => a.Email)
                .HasColumnName("email")
                .HasMaxLength(100);

            // Tạo chỉ mục độc nhất (Unique Index) để đảm bảo không trùng lặp Email
            builder.HasIndex(a => a.Email)
                .IsUnique();

            // 9. Số điện thoại (Phone): tối đa 20 ký tự, cho phép null
            builder.Property(a => a.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20);

            // 10. Trạng thái tài khoản (AccountStatus): tối đa 20 ký tự, giá trị mặc định là "Active", bắt buộc nhập
            builder.Property(a => a.AccountStatus)
                .HasColumnName("account_status")
                .HasMaxLength(20)
                .HasConversion<string>()
                .HasDefaultValue(AccountStatus.Active)
                .IsRequired();

            // 11. Thời điểm tạo bản ghi (CreatedAt - thuộc tính kế thừa từ BaseEntity):
            // Ánh xạ vào cột "created_at", mặc định sử dụng hàm lấy giờ hiện tại của DB (PostgreSQL: CURRENT_TIMESTAMP)
            builder.Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            // 12. Cấu hình cột RowVersion (thuộc tính kế thừa từ BaseEntity):
            // Dùng để kiểm soát xung đột đồng thời (Concurrency Control).
            // Khi Update/Delete, EF Core sẽ kiểm tra giá trị này khớp với DB hay không.
            // Nếu không khớp → nghĩa là bản ghi đã bị người khác sửa → ném DbUpdateConcurrencyException.
            builder.Property(a => a.RowVersion)
                .HasColumnName("row_version")
                .IsRowVersion();

            // 13. Cấu hình mối quan hệ N-1 (Nhiều Accounts thuộc về 1 Role)
            // - Sử dụng khóa ngoại RoleId liên kết với cột khóa chính của bảng Role
            // - Tránh xóa dây chuyền (DeleteBehavior.Restrict) - Không cho phép xóa Role nếu đang có Account tham chiếu tới
            builder.HasOne(a => a.Role)
                .WithMany(r => r.Accounts)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
