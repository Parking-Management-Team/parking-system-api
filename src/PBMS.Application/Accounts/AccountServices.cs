using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PBMS.Application.Accounts.DTOs;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;

namespace PBMS.Application.Accounts
{
    /// <summary>
    /// Thực thi các nghiệp vụ quản lý Tài khoản (Account).
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        // Khởi tạo và nhận Dependency Injection từ Repository
        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        /// <summary>
        /// Lấy toàn bộ danh sách tài khoản kèm theo RoleName từ bảng Role liên kết.
        /// </summary>
        public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
        {
            var accounts = await _accountRepository.GetAllWithRolesAsync();

            return accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                Username = a.Username,
                FullName = a.FullName,
                Email = a.Email,
                Phone = a.Phone,
                RoleId = a.RoleId,
                RoleName = a.Role?.RoleName ?? "Unknown",
                AccountStatus = a.AccountStatus,
                CreatedAt = a.CreatedAt
            });
        }

        /// <summary>
        /// Lấy chi tiết tài khoản theo ID kèm theo RoleName.
        /// </summary>
        public async Task<AccountDto?> GetAccountByIdAsync(int id)
        {
            var a = await _accountRepository.GetByIdWithRoleAsync(id);
            if (a == null) return null;

            return new AccountDto
            {
                Id = a.Id,
                Username = a.Username,
                FullName = a.FullName,
                Email = a.Email,
                Phone = a.Phone,
                RoleId = a.RoleId,
                RoleName = a.Role?.RoleName ?? "Unknown",
                AccountStatus = a.AccountStatus,
                CreatedAt = a.CreatedAt
            };
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản.
        /// CHỈ Admin mới được thay đổi RoleId hoặc AccountStatus.
        /// </summary>
        public async Task<bool> UpdateAccountAsync(int id, UpdateAccountDto dto, string currentUserRole)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null) return false;

            // 1. Kiểm tra bảo mật: Thay đổi vai trò (RoleId)
            if (dto.RoleId.HasValue && dto.RoleId.Value != account.RoleId)
            {
                if (currentUserRole != "Admin")
                {
                    throw new UnauthorizedAccessException("Only administrators are allowed to change account roles.");
                }
                account.RoleId = dto.RoleId.Value;
            }

            // 2. Kiểm tra bảo mật: Thay đổi trạng thái tài khoản (AccountStatus)
            if (!string.IsNullOrEmpty(dto.AccountStatus) && dto.AccountStatus != account.AccountStatus)
            {
                if (currentUserRole != "Admin")
                {
                    throw new UnauthorizedAccessException("Only administrators are allowed to change account status.");
                }
                account.AccountStatus = dto.AccountStatus;
            }

            // 3. Cập nhật thông tin cơ bản (User hay Admin đều tự làm được)
            account.FullName = dto.FullName ?? account.FullName;
            account.Phone = dto.Phone ?? account.Phone;

            _accountRepository.Update(account);
            await _accountRepository.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Khóa tài khoản bằng cách chuyển trạng thái hoạt động thành "Blocked" (Soft Delete).
        /// </summary>
        public async Task<bool> DeleteAccountAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null) return false;

            // Xóa mềm: Chuyển trạng thái sang Blocked
            account.AccountStatus = "Blocked";

            _accountRepository.Update(account);
            await _accountRepository.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Đổi mật khẩu tài khoản bằng cách kiểm tra mật khẩu cũ và mã hóa mật khẩu mới.
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto)
        {
            //1. Tìm tài khoản trong database
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null) return false;

            //2 Kiểm tra mật khẩu cũ có trùng khớp hay không
            bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(dto.OldPassword, account.PasswordHash);
            if (!isOldPasswordValid)
            {
                throw new UnauthorizedAccessException("Incorrect old password");
            }

            //3. Mã hoá mật khẩu mới bằng BCrypt và cập nhật lại 
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            //4. Lưu thay đổi vào database
            _accountRepository.Update(account);
            await _accountRepository.SaveChangesAsync();
            return true;
        }
    }
}
