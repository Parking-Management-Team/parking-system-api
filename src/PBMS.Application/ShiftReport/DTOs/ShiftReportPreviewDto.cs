using System;

namespace PBMS.Application.ShiftReport.DTOs;

/// <summary>
/// DTO chứa số liệu tạm tính (Preview) của ca trực hiện tại của Staff.
/// </summary>
public class ShiftReportPreviewDto
{
    public int StaffId { get; set; }
    public string StaffName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; } // Thời điểm hiện tại

    // Số liệu tạm tính từ hệ thống
    public int TotalCheckIn { get; set; }
    public int TotalCheckOut { get; set; }
    public decimal SystemRevenue { get; set; }
    public decimal ExpectedCashAmount { get; set; }
}
