using System.Collections.Generic;
using System.Threading.Tasks;
using PBMS.Application.Accounts.DTOs;

namespace PBMS.Application.Accounts
{
    /// <summary>
    /// Giao diện định nghĩa các nghiệp vụ (Business Logic) liên quan đến quản lý Account.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Lấy toàn bộ danh sách tài khoản kèm theo tên vai trò.
        /// </summary>
        Task<IEnumerable<AccountDto>> GetAllAccountsAsync();

        /// <summary>
        /// Lấy thông tin chi tiết một tài khoản kèm theo tên vai trò.
        /// </summary>
        Task<AccountDto?> GetAccountByIdAsync(int id);

        /// <summary>
        /// Cập nhật thông tin tài khoản và kiểm tra quyền hạn (Role-based).
        /// </summary>
        /// <param name="id">ID tài khoản cần sửa.</param>
        /// <param name="dto">Thông tin cần cập nhật.</param>
        /// <param name="currentUserRole">Vai trò của người thực hiện yêu cầu này.</param>
        Task<bool> UpdateAccountAsync(int id, UpdateAccountDto dto, string currentUserRole);

        /// <summary>
        /// Khóa tài khoản (Soft Delete).
        /// </summary>
        Task<bool> DeleteAccountAsync(int id);
    }
}
