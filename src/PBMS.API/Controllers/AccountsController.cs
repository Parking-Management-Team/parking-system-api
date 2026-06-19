using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Accounts;
using PBMS.Application.Accounts.DTOs;
using PBMS.Application.Common;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PBMS.API.Controllers
{
    /// <summary>
    /// API Controller quản lý thông tin tài khoản người dùng (Account CRUD).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu người dùng phải đăng nhập để gọi bất kỳ API nào ở đây
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách tài khoản (Chỉ Admin và Manager được xem).
        /// Route: GET /api/accounts
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(BaseResponse<IEnumerable<AccountDto>>.Ok(accounts));
        }

        /// <summary>
        /// Lấy chi tiết tài khoản theo ID.
        /// Route: GET /api/accounts/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            // Lấy ID và Role của người thực hiện request từ Claims JWT Token
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            // Người dùng thường chỉ được xem chính mình, Admin/Manager xem được tất cả
            if (currentUserRole != "Admin" && currentUserRole != "Manager" && currentUserId != id.ToString())
            {
                return Forbid(); // Trả về lỗi 403 Forbidden nếu không đủ quyền
            }

            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
            {
                return NotFound(BaseResponse<object>.Fail("Account not found."));
            }

            return Ok(BaseResponse<AccountDto>.Ok(account));
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản.
        /// Route: PUT /api/accounts/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountDto dto)
        {
            // Lấy ID và Role của người thực hiện request từ Claims JWT Token
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            // Người dùng thường chỉ được sửa chính mình, Admin được sửa tất cả
            if (currentUserRole != "Admin" && currentUserId != id.ToString())
            {
                return Forbid();
            }

            try
            {
                var success = await _accountService.UpdateAccountAsync(id, dto, currentUserRole);
                if (!success)
                {
                    return NotFound(BaseResponse<object>.Fail("Account not found."));
                }

                return Ok(BaseResponse<string>.Ok(id.ToString(), "Account updated successfully."));
            }
            catch (System.UnauthorizedAccessException ex)
            {
                return BadRequest(BaseResponse<object>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Khóa tài khoản (Chỉ Admin được phép khóa - Soft Delete).
        /// Route: DELETE /api/accounts/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var success = await _accountService.DeleteAccountAsync(id);
            if (!success)
            {
                return NotFound(BaseResponse<object>.Fail("Account not found."));
            }

            return Ok(BaseResponse<string>.Ok(id.ToString(), "Account blocked successfully."));
        }

        /// <summary>
        /// Tự deactivate tài khoản của chính mình (Soft Delete / Deactivate).
        /// Route: POST /api/accounts/{id}/deactivate
        /// </summary>
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateAccount(int id)
        {
            // Lấy ID của người thực hiện request từ Claims JWT Token
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            // Người dùng chỉ được phép tự deactivate tài khoản của chính mình
            if (currentUserId != id.ToString())
            {
                return Forbid();
            }

            var success = await _accountService.DeactivateAccountAsync(id);
            if (!success)
            {
                return NotFound(BaseResponse<object>.Fail("Account not found."));
            }

            return Ok(BaseResponse<string>.Ok(id.ToString(), "Account deactivated successfully."));
        }
        ///<summary>
        /// Đổi mật hẩu cho tài khoản đang đăng nhập 
        /// Route : Post /api/accounts/change-password
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            // 1. Lấy ID của tài khoản đang đăng nhập từ Claims trong JWT Token
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out int currentUserId))
            {
                return Unauthorized(BaseResponse<object>.Fail("Invalid user token. Please login again"));
            }
            try
            {
                //2. Goị tầng service để thực hiện đổi mật khẩu 
                var success = await _accountService.ChangePasswordAsync(currentUserId, dto);
                if (!success)
                {
                    return NotFound(BaseResponse<object>.Fail("Account not found."));

                }
                return Ok(BaseResponse<string>.Ok(currentUserId.ToString(), "Password changed succesfully"));
            }
            catch (System.UnauthorizedAccessException ex)
            {
                //Trả về lỗi 400 kèm thông báo mật khẩu cũ không đúng 
                return BadRequest(BaseResponse<object>.Fail(ex.Message));
            }
        }

    }
}
