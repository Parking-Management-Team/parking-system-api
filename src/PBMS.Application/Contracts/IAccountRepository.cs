using System.Threading.Tasks;
using PBMS.Domain.Entities;
using System.Collections.Generic;

namespace PBMS.Application.Contracts
{
    /// <summary>
    /// Hợp đồng Repository chuyên biệt cho thực thể Tài khoản (Account).
    /// Định nghĩa các truy vấn riêng ngoài các hàm CRUD chung trong IRepository.
    /// </summary>
    public interface IAccountRepository : IRepository<Account>
    {
        /// <summary>
        /// Tìm kiếm tài khoản trong hệ thống thông qua địa chỉ Email.
        /// </summary>
        /// <param name="email">Địa chỉ email của tài khoản cần tìm.</param>
        /// <returns>Thực thể Account nếu tồn tại; ngược lại là null.</returns>
        Task<Account?> GetByEmailAsync(string email);

        /// <summary>
        /// Lấy toàn bộ danh sách tài khoản kèm theo thông tin vai trò (Role).
        /// </summary>
        Task<IEnumerable<Account>> GetAllWithRolesAsync();

        /// <summary>
        /// Tìm kiếm tài khoản theo ID kèm theo thông tin vai trò (Role).
        /// </summary>
        Task<Account?> GetByIdWithRoleAsync(int id);
    }
}
