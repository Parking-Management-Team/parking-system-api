using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

/// <summary>
/// Hợp đồng Repository chuyên biệt cho thực thể Đăng ký vé tháng (MonthlySubscription).
/// </summary>
public interface IMonthlySubscriptionRepository : IRepository<MonthlySubscription>
{
    /// <summary>
    /// Lấy đăng ký vé tháng đang hoạt động (ACTIVE) của một thẻ gửi xe cụ thể.
    /// </summary>
    Task<MonthlySubscription?> GetActiveSubscriptionByCardIdAsync(int cardId);

    /// <summary>
    /// Lấy đăng ký vé tháng đang hoạt động (ACTIVE) của một xe cụ thể.
    /// </summary>
    Task<MonthlySubscription?> GetActiveSubscriptionByVehicleIdAsync(int vehicleId);

    /// <summary>
    /// Kiểm tra xe có đăng ký nào đang ACTIVE hoặc PENDING bị chồng lấn hay không.
    /// </summary>
    Task<bool> HasOverlapSubscriptionAsync(int vehicleId, int? excludeId = null);

    /// <summary>
    /// Đếm số lượng đăng ký vé tháng xe máy đang ACTIVE hoặc PENDING tại một tòa nhà cụ thể.
    /// Dùng để quản lý sức chứa (capacity) xe máy động.
    /// </summary>
    Task<int> GetActiveAndPendingMotorcycleSubscriptionsCountAsync(int buildingId);

    /// <summary>
    /// Lấy danh sách các đăng ký tháng ở trạng thái PENDING quá thời gian chờ thanh toán (timeout).
    /// </summary>
    Task<IEnumerable<MonthlySubscription>> GetTimeoutPendingSubscriptionsAsync(int timeoutMinutes);

    /// <summary>
    /// Lấy danh sách đăng ký vé tháng với phân trang và bộ lọc.
    /// </summary>
    Task<(IEnumerable<MonthlySubscription> Items, int TotalCount)> GetPagedAsync(
        int pageIndex, int pageSize, string? status, int? buildingId, int? accountId, string? licensePlate, string? cardCode);
}
