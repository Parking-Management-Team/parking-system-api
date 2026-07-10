using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ShiftReport.DTOs;

/// <summary>
/// Yêu cầu nộp báo cáo ca trực của nhân viên cổng.
/// </summary>
public class CreateShiftReportRequest
{
    [Required(ErrorMessage = "Staff ID is required.")]
    public int StaffId { get; set; }

    [Required(ErrorMessage = "Actual cash amount is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "Actual cash amount must be greater than or equal to 0.")]
    public decimal ActualCashAmount { get; set; }

    [MaxLength(200, ErrorMessage = "Note cannot exceed 200 characters.")]
    public string? Note { get; set; }
}
