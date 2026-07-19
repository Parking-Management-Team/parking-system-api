using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Auth.DTOs;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
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
            try
            {
                // Gọi tầng nghiệp vụ để kiểm tra thông tin đăng nhập
                var response = await _authService.LoginAsync(request);

                // Trả về kết quả thành công được bọc trong cấu trúc chuẩn BaseResponse
                return Ok(BaseResponse<LoginResponseDto>.Ok(response, "Login successful."));
            }
            catch (LoginOtpRequiredException ex)
            {
                // Trả về mã lỗi REQUIRE_LOGIN_OTP_VERIFICATION kèm email
                return Ok(new BaseResponse<LoginResponseDto>
                {
                    Success = false,
                    ErrorCode = "REQUIRE_LOGIN_OTP_VERIFICATION",
                    Message = ex.Message,
                    Data = new LoginResponseDto
                    {
                        Email = ex.Email
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(BaseResponse<LoginResponseDto>.Fail("UNAUTHORIZED", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(BaseResponse<LoginResponseDto>.Fail("BAD_REQUEST", ex.Message));
            }
        }

        /// <summary>
        /// API đăng nhập bằng Google OAuth2.
        /// Route: POST /api/auth/google
        /// </summary>
        /// <param name="request">DTO chứa Google ID Token từ Client gửi lên.</param>
        /// <returns>Thông tin Token JWT của hệ thống và tài khoản đăng nhập thành công.</returns>
        [HttpPost("google")]
        public async Task<ActionResult<BaseResponse<LoginResponseDto>>> LoginWithGoogle([FromBody] GoogleLoginRequest request)
        {
            try
            {
                // Gọi tầng nghiệp vụ để kiểm tra Google ID Token và đăng nhập/đăng ký lái xe
                var response = await _authService.LoginWithGoogleAsync(request);
                return Ok(BaseResponse<LoginResponseDto>.Ok(response, "Login successful."));
            }
            catch (GoogleSignupRequiredException ex)
            {
                // Trả về mã lỗi REQUIRE_OTP_VERIFICATION kèm email & tên đầy đủ của Google
                return Ok(new BaseResponse<LoginResponseDto>
                {
                    Success = false,
                    ErrorCode = "REQUIRE_OTP_VERIFICATION",
                    Message = ex.Message,
                    Data = new LoginResponseDto
                    {
                        Email = ex.Email,
                        FullName = ex.FullName
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(BaseResponse<LoginResponseDto>.Fail("UNAUTHORIZED", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(BaseResponse<LoginResponseDto>.Fail("BAD_REQUEST", ex.Message));
            }
        }
        [HttpPost("send-otp")]
        public async Task<ActionResult<BaseResponse<string>>> SendOtp([FromBody] SendOtpRequest request)
        {
            try
            {
                await _authService.SendOtpForRegisterAsync(request.Email);
                return Ok(BaseResponse<string>.Ok(null, "OTP code has been sent to your email successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(BaseResponse<string>.Fail("BAD_REQUEST", ex.Message));
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<BaseResponse<string>>> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var verificationToken = await _authService.VerifyOtpForRegisterAsync(request.Email, request.Otp);
                return Ok(BaseResponse<string>.Ok(verificationToken, "OTP verified successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(BaseResponse<string>.Fail("BAD_REQUEST", ex.Message));
            }
        }

        [HttpPost("register-verified")]
        public async Task<ActionResult<BaseResponse<string>>> RegisterVerified([FromBody] RegisterVerifiedRequest request)
        {
            try
            {
                await _authService.RegisterVerifiedUserAsync(request);
                return Ok(BaseResponse<string>.Ok(null, "Account registered successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(BaseResponse<string>.Fail("BAD_REQUEST", ex.Message));
            }
        }

        /// <summary>
        /// API xác thực OTP và hoàn tất đăng ký tài khoản từ liên kết Google.
        /// Route: POST /api/auth/google-verify-otp
        /// </summary>
        [HttpPost("google-verify-otp")]
        public async Task<ActionResult<BaseResponse<LoginResponseDto>>> VerifyGoogleOtp([FromBody] GoogleVerifyOtpRequest request)
        {
            try
            {
                var response = await _authService.VerifyGoogleOtpAndRegisterAsync(request);
                return Ok(BaseResponse<LoginResponseDto>.Ok(response, "Registration and login successful."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(BaseResponse<LoginResponseDto>.Fail("UNAUTHORIZED", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(BaseResponse<LoginResponseDto>.Fail("BAD_REQUEST", ex.Message));
            }
        }

        /// <summary>
        /// API xác thực OTP và hoàn tất đăng nhập cho email thường.
        /// Route: POST /api/auth/login-verify-otp
        /// </summary>
        [HttpPost("login-verify-otp")]
        public async Task<ActionResult<BaseResponse<LoginResponseDto>>> VerifyLoginOtp([FromBody] LoginVerifyOtpRequest request)
        {
            try
            {
                var response = await _authService.VerifyLoginOtpAsync(request);
                return Ok(BaseResponse<LoginResponseDto>.Ok(response, "Login successful."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(BaseResponse<LoginResponseDto>.Fail("UNAUTHORIZED", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(BaseResponse<LoginResponseDto>.Fail("BAD_REQUEST", ex.Message));
            }
        }
    }
}
