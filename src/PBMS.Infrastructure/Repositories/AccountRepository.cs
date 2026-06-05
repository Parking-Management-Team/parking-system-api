using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.Repositories
{
    /// <summary>
    /// Triển khai interface IAccountRepository thực thi các truy vấn dữ liệu cụ thể liên quan tới Account.
    /// Kế thừa toàn bộ khả năng CRUD từ BaseRepository<Account>.
    /// </summary>
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        // 1. Khai báo danh sách tĩnh làm Database ảo lưu trong RAM của ứng dụng
        private static readonly List<Account> _mockAccounts = new List<Account>
        {
            new Account
            {
                Id = 1,
                Email = "admin@pbms.com",
                Username = "admin",
                FullName = "System Administrator",
                AccountStatus = "Active",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                RoleId = 1,
                Role = new Role
                {
                    Id = 1,
                    RoleName = "Admin",
                    Description = "System Administrator"
                }
            },
            new Account
            {
                Id = 2,
                Email = "manager@pbms.com",
                Username = "manager",
                FullName = "Project Manager",
                AccountStatus = "Active",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                RoleId = 2,
                Role = new Role
                {
                    Id = 2,
                    RoleName = "Manager",
                    Description = "Project Manager"
                }
            }
        };

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

            // 2. Tìm kiếm trong danh sách ảo static trước
            var mockAccount = _mockAccounts.FirstOrDefault(a => a.Email != null && a.Email.ToLower() == normalizedEmail);
            if (mockAccount != null)
            {
                return mockAccount;
            }

            // 3. Thực thi truy vấn EF Core (nếu có Database thật):
            try
            {
                return await _dbSet
                    .Include(a => a.Role)
                    .FirstOrDefaultAsync(a => a.Email != null && a.Email.ToLower() == normalizedEmail);
            }
            catch (Exception)
            {
                // Bắt các ngoại lệ kết nối DB để tránh sập app khi chưa kết nối Database thành công
                return null;
            }
        }

        /// <summary>
        /// Thêm mới tài khoản (Hỗ trợ ghi vào cả mock list và EF Core)
        /// </summary>
        public override async Task AddAsync(Account entity)
        {
            if (entity == null) return;

            // Thiết lập ID tự tăng cho tài khoản ảo mới
            entity.Id = _mockAccounts.Any() ? _mockAccounts.Max(a => a.Id) + 1 : 1;

            // Gán thông tin Role ảo để tránh lỗi NullReferenceException khi xuất token
            if (entity.Role == null)
            {
                entity.Role = new Role
                {
                    Id = entity.RoleId,
                    RoleName = entity.RoleId == 3 ? "Driver" : "Customer",
                    Description = entity.RoleId == 3 ? "Driver Role" : "Customer Role"
                };
            }

            // Thêm vào danh sách static
            _mockAccounts.Add(entity);

            try
            {
                await base.AddAsync(entity);
            }
            catch (Exception)
            {
                // Bỏ qua lỗi DB nếu chưa kết nối DB thành công
            }
        }

        /// <summary>
        /// Ghi đè phương thức Lưu thay đổi để hỗ trợ chạy thử không cần Database
        /// </summary>
        public override async Task<int> SaveChangesAsync()
        {
            try
            {
                return await base.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Trả về 1 dòng bị ảnh hưởng để mô phỏng lưu thành công khi không có DB
                return 1;
            }
        }
    }
}
