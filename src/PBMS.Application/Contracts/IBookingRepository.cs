using BookingEntity = PBMS.Domain.Entities.Booking;

namespace PBMS.Application.Contracts;

/// <summary>
/// Interface repository cho entity Booking.
/// Cung cấp các truy vấn chuyên biệt phục vụ nghiệp vụ đặt chỗ trước.
/// </summary>
public interface IBookingRepository : IRepository<BookingEntity>
{
    /// <summary>
    /// Lấy danh sách tất cả Booking kèm theo thông tin Account, Vehicle, Building.
    /// </summary>
    Task<IEnumerable<BookingEntity>> GetAllWithDetailsAsync();

    /// <summary>
    /// Lấy Booking theo ID kèm thông tin liên quan (Account, Vehicle, Building).
    /// </summary>
    Task<BookingEntity?> GetByIdWithDetailsAsync(int id);

    /// <summary>
    /// Lấy danh sách Booking của một Account cụ thể.
    /// </summary>
    Task<IEnumerable<BookingEntity>> GetByAccountIdAsync(int accountId);

    /// <summary>
    /// Lấy danh sách Booking của một Building cụ thể (dùng cho quản lý).
    /// </summary>
    Task<IEnumerable<BookingEntity>> GetByBuildingIdAsync(int buildingId);

    /// <summary>
    /// Đếm số Booking đang "chiếm" chỗ tại một Building cho một loại xe cụ thể.
    /// Tính các Booking có Status = Pending hoặc Confirmed (chưa check-in, chưa hủy).
    /// Dùng để kiểm tra General Capacity còn lại trước khi tạo Booking mới.
    /// </summary>
    Task<int> GetActiveBookingsCountAsync(int buildingId, int vehicleTypeId);
}
