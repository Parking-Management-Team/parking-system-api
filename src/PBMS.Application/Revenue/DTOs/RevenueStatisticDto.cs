using System;
using System.Collections.Generic;

namespace PBMS.Application.Revenue.DTOs;

/// <summary>
/// DTO thông tin thống kê doanh thu theo chu kỳ.
/// </summary>
public class RevenueStatisticDto
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public string BuildingName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string PeriodType { get; set; } = null!;

    /// <summary>
    /// Khóa ngoại loại xe. Nếu là null thì đại diện cho tổng doanh thu toàn bãi.
    /// </summary>
    public int? VehicleTypeId { get; set; }
    public string VehicleTypeName { get; set; } = null!; // "Motorcycle", "Car" hoặc "Total Revenue"

    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int TotalSessions { get; set; }
    public int TotalSubscriptions { get; set; }

    /// <summary>
    /// Danh sách chi tiết các giao dịch đối soát cấu thành nên doanh thu này.
    /// </summary>
    public List<RevenuePaymentDetailDto> Payments { get; set; } = new();
}
