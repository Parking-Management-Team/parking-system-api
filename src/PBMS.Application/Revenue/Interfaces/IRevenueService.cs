using System.Threading.Tasks;
using PBMS.Application.Common;
using PBMS.Application.Revenue.DTOs;

namespace PBMS.Application.Revenue.Interfaces;

/// <summary>
/// Giao diện dịch vụ thống kê doanh thu (Revenue Service).
/// </summary>
public interface IRevenueService
{
    /// <summary>
    /// Lấy danh sách thống kê doanh thu theo bộ lọc và phân trang.
    /// </summary>
    Task<PagedResult<RevenueStatisticDto>> GetRevenueStatisticsAsync(RevenueFilterDto filter, int pageIndex, int pageSize);

    /// <summary>
    /// Lấy thông tin thống kê doanh thu chi tiết theo ID (kèm danh sách thanh toán).
    /// </summary>
    Task<RevenueStatisticDto> GetRevenueStatisticByIdAsync(int id);

    /// <summary>
    /// Trigger cập nhật doanh thu tự động khi có thanh toán thành công (PAID).
    /// </summary>
    /// <param name="paymentId">ID của giao dịch thanh toán vừa thành công</param>
    Task UpdateRevenueAfterPaymentAsync(int paymentId);
}
