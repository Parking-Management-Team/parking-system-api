using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Auth.DTOs;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Common;
using System;
using System.Threading.Tasks;

namespace PBMS.API.Controllers
{
    /// <summary>
    /// API Controller xử lý các yêu cầu liên quan tới Xác thực và Phân quyền (Authentication & Authorization).
    /// Endpoint chính: /api/auth
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Constructor thực hiện tiêm dịch vụ IAuthService thông qua Dependency Injection
        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// API đăng nhập tài khoản hệ thống.
        /// Route: POST /api/auth/login
        /// </summary>
        /// <param name="request">DTO chứa thông tin Email và Mật khẩu từ Client gửi lên.</param>
        /// <returns>Thông tin Token JWT và tài khoản nếu đăng nhập thành công.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<BaseResponse<LoginResponseDto>>> Login([FromBody] LoginRequest request)
        {
            // Gọi tầng nghiệp vụ để kiểm tra thông tin đăng nhập
            var response = await _authService.LoginAsync(request);

            // Trả về kết quả thành công được bọc trong cấu trúc chuẩn BaseResponse
            return Ok(BaseResponse<LoginResponseDto>.Ok(response, "Login successful."));
        }
    }
}
