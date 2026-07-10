using System;

namespace PBMS.Application.ShiftReport.DTOs;

/// <summary>
/// DTO chứa thông tin chi tiết báo cáo ca trực (Shift Report).
/// </summary>
public class ShiftReportDto
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Số liệu hệ thống
    public int TotalCheckIn { get; set; }
    public int TotalCheckOut { get; set; }
    public decimal SystemRevenue { get; set; }
    public decimal ExpectedCashAmount { get; set; }

    // Số liệu tự khai
    public decimal ActualCashAmount { get; set; }
    public decimal DifferenceAmount { get; set; }
    public string? Note { get; set; }

    // Trạng thái phê duyệt
    public string Status { get; set; } = null!;
    public int? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
