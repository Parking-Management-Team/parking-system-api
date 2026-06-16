using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

/// <summary>
/// Hợp đồng Repository chuyên biệt cho thực thể Đăng ký vé tháng (MonthlySubscription).
/// </summary>
public interface IMonthlySubscriptionRepository : IRepository<MonthlySubscription>
{
    /// <summary>
    /// Tìm đăng ký vé tháng đang hoạt động (ACTIVE) liên kết với mã thẻ (CardCode).
    /// </summary>
    /// <param name="cardCode">Mã thẻ gửi xe.</param>
    /// <returns>Bản ghi đăng ký tháng hoạt động hoặc null.</returns>
    Task<MonthlySubscription?> GetActiveSubscriptionByCardCodeAsync(string cardCode);

    /// <summary>
    /// Tìm đăng ký vé tháng đang hoạt động (ACTIVE) liên kết với ID phương tiện (VehicleId).
    /// </summary>
    /// <param name="vehicleId">ID phương tiện xe.</param>
    /// <returns>Bản ghi đăng ký tháng hoạt động hoặc null.</returns>
    Task<MonthlySubscription?> GetActiveSubscriptionByVehicleIdAsync(int vehicleId);

    /// <summary>
    /// Kiểm tra xem một thẻ gửi xe đã được gán cho một hồ sơ vé tháng ACTIVE nào khác hay chưa.
    /// </summary>
    /// <param name="cardId">ID của thẻ gửi xe.</param>
    /// <returns>true nếu thẻ đã được gán cho vé tháng khác đang hoạt động; ngược lại false.</returns>
    Task<bool> IsCardAssignedToActiveSubscriptionAsync(int cardId);
}
