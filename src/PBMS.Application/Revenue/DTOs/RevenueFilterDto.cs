using System;

namespace PBMS.Application.Revenue.DTOs;

/// <summary>
/// DTO chứa bộ lọc tìm kiếm thống kê doanh thu.
/// </summary>
public class RevenueFilterDto
{
    public int? BuildingId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Loại chu kỳ: "DAILY" (mặc định), "MONTHLY", "YEARLY".
    /// </summary>
    public string PeriodType { get; set; } = "DAILY";
}
