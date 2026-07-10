using System.Threading.Tasks;
using PBMS.Application.Common;
using PBMS.Application.ShiftReport.DTOs;

namespace PBMS.Application.ShiftReport.Interfaces;

/// <summary>
/// Giao diện dịch vụ xử lý nghiệp vụ Báo cáo ca trực (Shift Report).
/// </summary>
public interface IShiftReportService
{
    /// <summary>
    /// Xem trước số liệu ca hiện tại của nhân viên dựa theo thời gian thực tế.
    /// </summary>
    Task<BaseResponse<ShiftReportPreviewDto>> PreviewShiftReportAsync(int staffId);

    /// <summary>
    /// Nộp báo cáo ca trực chính thức lên hệ thống.
    /// </summary>
    Task<BaseResponse<ShiftReportDto>> SubmitShiftReportAsync(CreateShiftReportRequest request);

    /// <summary>
    /// Phê duyệt hoặc từ chối đối soát báo cáo ca trực (dành cho Manager).
    /// </summary>
    Task<BaseResponse<ShiftReportDto>> ApproveShiftReportAsync(int reportId, int managerId, bool approve);

    /// <summary>
    /// Lấy danh sách các báo cáo ca trực có phân trang.
    /// </summary>
    Task<BaseResponse<PagedResult<ShiftReportDto>>> GetShiftReportsPagedAsync(int pageIndex, int pageSize);
}
