using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.Repositories
{
    /// <summary>
    /// Triển khai interface IAccountRepository thực thi các truy vấn dữ liệu cụ thể liên quan tới Account.
    /// Kế thừa toàn bộ khả năng CRUD từ BaseRepository<Account>.
    /// </summary>
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        // Constructor truyền DbContext cho lớp cha (BaseRepository) để thực hiện kết nối
        public AccountRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Tìm kiếm tài khoản dựa vào địa chỉ Email và nạp kèm thông tin vai trò (Role).
        /// </summary>
        /// <param name="email">Email cần tìm kiếm</param>
        /// <returns>Tài khoản nếu tìm thấy kèm theo thông tin Role; ngược lại trả về null.</returns>
        public async Task<Account?> GetByEmailAsync(string email)
        {
            // 1. Kiểm tra nhanh chuỗi đầu vào (tránh lỗi null hoặc rỗng)
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            // 2. Chuyển chuỗi về chữ thường và cắt khoảng trắng thừa để việc so khớp chính xác
            var normalizedEmail = email.Trim().ToLower();

            // 3. Thực thi truy vấn EF Core:
            // - Dùng .Include(a => a.Role) để thực hiện JOIN bảng role ở mức DB và nạp sẵn đối tượng Role vào Account (Eager Loading)
            // - Phục vụ trực tiếp cho việc trích xuất Claims (RoleName) khi tạo mã JWT Token sau này.
            return await _dbSet
                .Include(a => a.Role) 
                .FirstOrDefaultAsync(a => a.Email != null && a.Email.ToLower() == normalizedEmail);
        }
    }
}
