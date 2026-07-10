using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ShiftReport.DTOs;
using PBMS.Application.ShiftReport.Interfaces;
using PBMS.Domain.Entities;

namespace PBMS.Application.ShiftReport.Services;

public class ShiftReportService : IShiftReportService
{
    private readonly IShiftReportRepository _shiftReportRepository;
    private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IRepository<Notification> _notificationRepository;

    public ShiftReportService(
        IShiftReportRepository shiftReportRepository,
        IRepository<PBMS.Domain.Entities.ParkingSession> sessionRepository,
        IPaymentRepository paymentRepository,
        IAccountRepository accountRepository,
        IRepository<Notification> notificationRepository)
    {
        _shiftReportRepository = shiftReportRepository;
        _sessionRepository = sessionRepository;
        _paymentRepository = paymentRepository;
        _accountRepository = accountRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<BaseResponse<ShiftReportPreviewDto>> PreviewShiftReportAsync(int staffId)
    {
        var staff = await _accountRepository.GetByIdAsync(staffId);
        if (staff == null)
        {
            throw new NotFoundException("Staff Account", staffId);
        }

        var endTime = DateTime.UtcNow.AddHours(7);
        var startTime = await CalculateShiftStartTimeAsync(staffId);

        // Đếm Check-in và Check-out
        int totalCheckIn = await _sessionRepository.CountAsync(s => 
            s.InStaffId == staffId && 
            s.CheckInTime >= startTime && 
            s.CheckInTime <= endTime);

        int totalCheckOut = await _sessionRepository.CountAsync(s => 
            s.OutStaffId == staffId && 
            s.CheckOutTime >= startTime && 
            s.CheckOutTime <= endTime);

        // Tính doanh thu
        var (expectedCash, systemRevenue) = await CalculateRevenueInShiftAsync(staffId, startTime, endTime);

        var preview = new ShiftReportPreviewDto
        {
            StaffId = staffId,
            StaffName = staff.FullName ?? staff.Username,
            StartTime = startTime,
            EndTime = endTime,
            TotalCheckIn = totalCheckIn,
            TotalCheckOut = totalCheckOut,
            SystemRevenue = systemRevenue,
            ExpectedCashAmount = expectedCash
        };

        return BaseResponse<ShiftReportPreviewDto>.Ok(preview, "Shift report preview generated successfully.");
    }

    public async Task<BaseResponse<ShiftReportDto>> SubmitShiftReportAsync(CreateShiftReportRequest request)
    {
        var staff = await _accountRepository.GetByIdAsync(request.StaffId);
        if (staff == null)
        {
            throw new NotFoundException("Staff Account", request.StaffId);
        }

        var endTime = DateTime.UtcNow.AddHours(7);
        var startTime = await CalculateShiftStartTimeAsync(request.StaffId);

        // Đếm Check-in và Check-out
        int totalCheckIn = await _sessionRepository.CountAsync(s => 
            s.InStaffId == request.StaffId && 
            s.CheckInTime >= startTime && 
            s.CheckInTime <= endTime);

        int totalCheckOut = await _sessionRepository.CountAsync(s => 
            s.OutStaffId == request.StaffId && 
            s.CheckOutTime >= startTime && 
            s.CheckOutTime <= endTime);

        // Tính doanh thu
        var (expectedCash, systemRevenue) = await CalculateRevenueInShiftAsync(request.StaffId, startTime, endTime);

        decimal difference = request.ActualCashAmount - expectedCash;

        var report = new PBMS.Domain.Entities.ShiftReport
        {
            StaffId = request.StaffId,
            StartTime = startTime,
            EndTime = endTime,
            TotalCheckIn = totalCheckIn,
            TotalCheckOut = totalCheckOut,
            SystemRevenue = systemRevenue,
            ExpectedCashAmount = expectedCash,
            ActualCashAmount = request.ActualCashAmount,
            DifferenceAmount = difference,
            Note = request.Note,
            Status = "Submitted"
        };

        await _shiftReportRepository.AddAsync(report);
        await _shiftReportRepository.SaveChangesAsync();

        // 6. Gửi thông báo cho toàn bộ Managers trong hệ thống
        try
        {
            var allAccounts = await _accountRepository.GetAllWithRolesAsync();
            var managers = allAccounts.Where(a => string.Equals(a.Role?.RoleName, "Manager", StringComparison.OrdinalIgnoreCase));

            foreach (var manager in managers)
            {
                var notification = new Notification
                {
                    AccountId = manager.Id,
                    Title = "Yêu cầu phê duyệt báo cáo ca trực",
                    Message = $"Nhân viên {staff.FullName ?? staff.Username} đã nộp báo cáo ca trực lúc {endTime:dd/MM/yyyy HH:mm:ss} và đang chờ phê duyệt.",
                    CreatedAt = DateTime.UtcNow.AddHours(7)
                };
                await _notificationRepository.AddAsync(notification);
            }
            await _notificationRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Bỏ qua lỗi gửi thông báo để tránh rollback transaction chính
        }

        return BaseResponse<ShiftReportDto>.Ok(MapToDto(report), "Shift report submitted successfully.");
    }

    public async Task<BaseResponse<ShiftReportDto>> ApproveShiftReportAsync(int reportId, int managerId, bool approve)
    {
        var report = await _shiftReportRepository.GetByIdAsync(reportId);
        if (report == null)
        {
            throw new NotFoundException("ShiftReport", reportId);
        }

        if (report.Status != "Submitted")
        {
            return BaseResponse<ShiftReportDto>.Fail("INVALID_STATUS", $"Cannot approve/reject report in '{report.Status}' status.");
        }

        var manager = await _accountRepository.GetByIdAsync(managerId);
        if (manager == null)
        {
            throw new NotFoundException("Manager Account", managerId);
        }

        report.Status = approve ? "Approved" : "Rejected";
        report.ApprovedById = managerId;
        report.ApprovedAt = DateTime.UtcNow.AddHours(7);

        _shiftReportRepository.Update(report);
        await _shiftReportRepository.SaveChangesAsync();

        // Reload data to map full info
        var updated = await _shiftReportRepository.GetByIdAsync(reportId);
        return BaseResponse<ShiftReportDto>.Ok(MapToDto(updated ?? report), $"Shift report has been {(approve ? "Approved" : "Rejected")} successfully.");
    }

    public async Task<BaseResponse<PagedResult<ShiftReportDto>>> GetShiftReportsPagedAsync(int pageIndex, int pageSize)
    {
        var (items, totalCount) = await _shiftReportRepository.GetPagedWithDetailsAsync(pageIndex, pageSize);
        var dtos = items.Select(MapToDto).ToList();
        
        var pagedResult = PagedResult<ShiftReportDto>.Create(dtos, totalCount, pageIndex, pageSize);
        return BaseResponse<PagedResult<ShiftReportDto>>.Ok(pagedResult);
    }

    // -----------------------------------------------------------------------
    // HELPERS
    // -----------------------------------------------------------------------

    private async Task<DateTime> CalculateShiftStartTimeAsync(int staffId)
    {
        // 1. Tìm báo cáo ca trực gần nhất của nhân viên này
        var lastReport = await _shiftReportRepository.GetLastReportByStaffIdAsync(staffId);
        if (lastReport != null)
        {
            return lastReport.EndTime;
        }

        // 2. Nếu là ca trực đầu tiên, tìm ParkingSession đầu tiên do nhân viên này cho xe vào/ra
        var firstSession = (await _sessionRepository.FindAsync(s => 
            s.InStaffId == staffId || s.OutStaffId == staffId))
            .OrderBy(s => s.CheckInTime)
            .FirstOrDefault();

        if (firstSession != null)
        {
            return firstSession.CheckInTime;
        }

        // 3. Nếu chưa có bất kỳ dữ liệu hoạt động nào, mặc định là 8 tiếng trước
        return DateTime.UtcNow.AddHours(7).AddHours(-8);
    }

    private async Task<(decimal ExpectedCash, decimal SystemRevenue)> CalculateRevenueInShiftAsync(int staffId, DateTime start, DateTime end)
    {
        // Lấy tất cả lượt gửi xe mà nhân viên này thực hiện check-out và hoàn thành trong ca trực
        var sessions = await _sessionRepository.FindAsync(s => 
            s.OutStaffId == staffId && 
            s.CheckOutTime >= start && 
            s.CheckOutTime <= end);

        var sessionIds = sessions.Select(s => s.Id).ToList();
        if (!sessionIds.Any())
        {
            return (0, 0);
        }

        // Lấy tất cả các giao dịch thanh toán thành công liên quan đến các lượt xe trên
        var payments = await _paymentRepository.FindAsync(p => 
            p.PaymentStatus == "PAID" && 
            p.SessionId.HasValue && 
            sessionIds.Contains(p.SessionId.Value));

        decimal expectedCash = payments
            .Where(p => string.Equals(p.PaymentMethod, "CASH", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Amount);

        decimal systemRevenue = payments
            .Sum(p => p.Amount);

        return (expectedCash, systemRevenue);
    }

    private static ShiftReportDto MapToDto(PBMS.Domain.Entities.ShiftReport report)
    {
        return new ShiftReportDto
        {
            Id = report.Id,
            StaffId = report.StaffId,
            StaffName = report.Staff?.FullName ?? report.Staff?.Username ?? $"Staff #{report.StaffId}",
            StartTime = report.StartTime,
            EndTime = report.EndTime,
            TotalCheckIn = report.TotalCheckIn,
            TotalCheckOut = report.TotalCheckOut,
            SystemRevenue = report.SystemRevenue,
            ExpectedCashAmount = report.ExpectedCashAmount,
            ActualCashAmount = report.ActualCashAmount,
            DifferenceAmount = report.DifferenceAmount,
            Note = report.Note,
            Status = report.Status,
            ApprovedById = report.ApprovedById,
            ApprovedByName = report.ApprovedBy?.FullName ?? report.ApprovedBy?.Username,
            ApprovedAt = report.ApprovedAt,
            CreatedAt = report.CreatedAt
        };
    }
}
