using PBMS.Application.Booking.DTOs;

namespace PBMS.Application.Booking.Interfaces;

/// <summary>
/// Interface định nghĩa các nghiệp vụ quản lý Đặt chỗ trước (Booking).
///
/// Các chức năng CRUD:
///   - Create: Tạo Booking mới (PENDING, kiểm tra capacity và tính deposit)
///   - Read:   Lấy thông tin Booking (theo ID, theo Account, theo Building, tất cả)
///   - Update: Cập nhật thời gian đặt chỗ (chỉ khi PENDING)
///   - Delete: Hủy Booking (Soft cancel, giải phóng capacity)
/// </summary>
public interface IBookingService
{
    // -----------------------------------------------------------------------
    // CREATE
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tạo Booking mới với trạng thái PENDING.
    ///
    /// Business Rules:
    ///   - PlannedCheckinTime phải cách Now ít nhất 15 phút (UTC).
    ///   - Building phải còn General Capacity cho loại xe tương ứng.
    ///   - Deposit Fee = BasePrice của PricingWindow tại giờ PlannedCheckinTime.
    ///   - PaymentDeadline = Now + 15 phút.
    ///   - CheckinGraceUntil = PlannedCheckinTime + 30 phút.
    /// </summary>
    Task<BookingDto> CreateBookingAsync(CreateBookingRequest request);

    // -----------------------------------------------------------------------
    // READ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy danh sách toàn bộ Booking (dùng cho Admin/Staff quản lý).
    /// </summary>
    Task<List<BookingDto>> GetAllBookingsAsync();

    /// <summary>
    /// Lấy thông tin chi tiết Booking theo ID.
    /// Ném DomainException "BOOKING_NOT_FOUND" nếu không tìm thấy.
    /// </summary>
    Task<BookingDto> GetBookingByIdAsync(int id);

    /// <summary>
    /// Lấy danh sách Booking của một Account cụ thể.
    /// </summary>
    Task<List<BookingDto>> GetBookingsByAccountIdAsync(int accountId);

    /// <summary>
    /// Lấy danh sách Booking của một Building cụ thể.
    /// </summary>
    Task<List<BookingDto>> GetBookingsByBuildingIdAsync(int buildingId);

    // -----------------------------------------------------------------------
    // UPDATE
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật thời gian đặt chỗ của một Booking đang PENDING.
    /// Nếu Booking không ở trạng thái PENDING → ném DomainException "BOOKING_NOT_EDITABLE".
    /// Thời gian mới phải thỏa mãn ràng buộc 1h-8h tương tự CreateBooking.
    /// Deposit Fee sẽ được tính lại theo giờ mới.
    /// </summary>
    Task<BookingDto> UpdateBookingAsync(int id, UpdateBookingRequest request);

    // -----------------------------------------------------------------------
    // DELETE / CANCEL
    // -----------------------------------------------------------------------

    /// <summary>
    /// Hủy Booking (Soft cancel — chuyển trạng thái sang "Cancelled").
    /// Capacity tạm giữ sẽ được giải phóng ngay khi hủy.
    /// Chỉ hủy được khi Booking đang ở trạng thái Pending hoặc Confirmed.
    /// Nếu Booking đã CheckedIn/NoShow/Expired → ném DomainException "BOOKING_NOT_CANCELLABLE".
    /// </summary>
    Task CancelBookingAsync(int id, string? reason = null);

    // -----------------------------------------------------------------------
    // INTERNAL OPERATIONS (gọi từ các Service khác)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Dọn dẹp các Booking hết hạn:
    ///   - PENDING quá hạn thanh toán tiền cọc -> Expired
    ///   - CONFIRMED quá hạn grace period check-in -> NoShow
    /// Dùng cho background job chạy định kỳ.
    /// </summary>
    Task CleanupExpiredBookingsAsync();
}
