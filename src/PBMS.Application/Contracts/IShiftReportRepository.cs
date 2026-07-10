using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftReportEntity = PBMS.Domain.Entities.ShiftReport;

namespace PBMS.Application.Contracts;

/// <summary>
/// Giao diện kho dữ liệu cho thực thể Báo cáo ca trực (ShiftReport).
/// </summary>
public interface IShiftReportRepository : IRepository<ShiftReportEntity>
{
    /// <summary>
    /// Lấy báo cáo ca trực gần nhất của một nhân viên.
    /// </summary>
    Task<ShiftReportEntity?> GetLastReportByStaffIdAsync(int staffId);

    /// <summary>
    /// Lấy danh sách báo cáo ca trực phân trang cùng thông tin chi tiết nhân viên và quản lý duyệt.
    /// </summary>
    Task<(IEnumerable<ShiftReportEntity> Items, int TotalCount)> GetPagedWithDetailsAsync(int pageIndex, int pageSize);
}
