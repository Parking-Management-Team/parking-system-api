namespace PBMS.Application.Auth.Interfaces;

/// <summary>
/// Giao diện lấy thông tin người dùng hiện tại đang thực hiện request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID của người dùng hiện tại (lấy từ JWT claim).
    /// Trả về null nếu request chưa được xác thực.
    /// </summary>
    int? UserId { get; }
}
