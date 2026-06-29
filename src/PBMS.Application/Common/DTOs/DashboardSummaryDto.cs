namespace PBMS.Application.Common.DTOs;

public class DashboardSummaryDto
{
    public int TotalActiveSessions { get; set; }
    public int ExpectedBookingsToday { get; set; }
    public double OccupancyRate { get; set; }
    public int ActiveIncidentsCount { get; set; }
    public int ActiveMonthlySubscriptions { get; set; }
}
