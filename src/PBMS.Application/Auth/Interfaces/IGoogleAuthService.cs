using System.Threading.Tasks;

namespace PBMS.Application.Auth.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ xác thực bên thứ 3 Google Oauth2
    /// Định nghĩa các phương thức giao tiếp mà không phụ thuộc vào thư viện cụ thể 
    /// </summary>

    public interface IGoogleAuthService
    {
        /// <summary>
        /// Xác thực tính hợp lệ của Google ID Token và trích xuất thông tin người dùng.
        ///  </summary>
        /// <param name = "idToken"> Mã ID Token dạng JWT do cilent gửi lên. </param>
        /// <returns> Thông tin người dùng Google nếu token hợp lệ ; ngược lại trả về null. </returns>
        Task<GoogleUserInfo?> VerifyTokenAsync(string idToken);

    }

    /// <summary>
    /// Thông tin trích xuất từ Google ID Token
    /// </summary>
    public class GoogleUserInfo
    {
        // Địa chỉ email của tài khoản Google (dùng để tìm kiếm hoặc tạo mới tài khoản trong hệ thống)
        public string Email { get; set; } = null!;
        // Tên hiển thị của người dùng Google (tên đầy đủ)
        public string Name { get; set; } = null!;
        // URL ảnh đại diện của người dùng Google
        public string? PictureUrl { get; set; }
    }
}
