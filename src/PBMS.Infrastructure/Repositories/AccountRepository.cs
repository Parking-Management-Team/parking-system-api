using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;
using System.Collections.Generic;
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
        public async Task<Account?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail = email.Trim().ToLower();

            // Truy vấn trực tiếp từ Database và nạp kèm thông tin Role
            return await _dbSet
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Email != null && a.Email.ToLower() == normalizedEmail);
        }

        /// <summary>
        /// Lấy toàn bộ danh sách tài khoản kèm theo thông tin vai trò (Role).
        /// </summary>
        public async Task<IEnumerable<Account>> GetAllWithRolesAsync()
        {
            return await _dbSet
                .Include(a => a.Role)
                .ToListAsync();
        }

        /// <summary>
        /// Tìm kiếm tài khoản theo ID kèm theo thông tin vai trò (Role).
        /// </summary>
        public async Task<Account?> GetByIdWithRoleAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}
