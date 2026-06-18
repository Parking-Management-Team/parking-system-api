using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Application.Revenue.DTOs;
using PBMS.Application.Revenue.Interfaces;

namespace PBMS.Application.Revenue.Services;

/// <summary>
/// Triển khai dịch vụ thống kê doanh thu.
/// </summary>
public class RevenueService : IRevenueService
{
    private readonly IRepository<PBMS.Domain.Entities.RevenueStatistic> _revenueStatisticRepository;
    private readonly IRepository<PBMS.Domain.Entities.RevenueStatisticPayment> _revenueStatisticPaymentRepository;
    private readonly IRepository<PBMS.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<PBMS.Domain.Entities.Building> _buildingRepository;
    private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepository;
    private readonly IRepository<PBMS.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<PBMS.Domain.Entities.MonthlySubscription> _subscriptionRepository;
    private readonly IRepository<PBMS.Domain.Entities.Vehicle> _vehicleRepository;
    private readonly IRepository<PBMS.Domain.Entities.VehicleType> _vehicleTypeRepository;

    public RevenueService(
        IRepository<PBMS.Domain.Entities.RevenueStatistic> revenueStatisticRepository,
        IRepository<PBMS.Domain.Entities.RevenueStatisticPayment> revenueStatisticPaymentRepository,
        IRepository<PBMS.Domain.Entities.Payment> paymentRepository,
        IRepository<PBMS.Domain.Entities.Building> buildingRepository,
        IRepository<PBMS.Domain.Entities.ParkingSession> sessionRepository,
        IRepository<PBMS.Domain.Entities.Booking> bookingRepository,
        IRepository<PBMS.Domain.Entities.MonthlySubscription> subscriptionRepository,
        IRepository<PBMS.Domain.Entities.Vehicle> vehicleRepository,
        IRepository<PBMS.Domain.Entities.VehicleType> vehicleTypeRepository)
    {
        _revenueStatisticRepository = revenueStatisticRepository;
        _revenueStatisticPaymentRepository = revenueStatisticPaymentRepository;
        _paymentRepository = paymentRepository;
        _buildingRepository = buildingRepository;
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _subscriptionRepository = subscriptionRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
    }

    /// <summary>
    /// Lấy danh sách thống kê doanh thu phân trang.
    /// </summary>
    public async Task<PagedResult<RevenueStatisticDto>> GetRevenueStatisticsAsync(RevenueFilterDto filter, int pageIndex, int pageSize)
    {
        // 1. Lấy danh sách thô từ Database
        var stats = await _revenueStatisticRepository.FindAsync(rs =>
            (!filter.BuildingId.HasValue || rs.BuildingId == filter.BuildingId.Value) &&
            (!filter.StartDate.HasValue || rs.StartDate >= filter.StartDate.Value) &&
            (!filter.EndDate.HasValue || rs.EndDate <= filter.EndDate.Value) &&
            (rs.PeriodType == filter.PeriodType)
        );

        var orderedStats = stats.OrderByDescending(rs => rs.StartDate).ToList();
        var totalCount = orderedStats.Count;

        // Phân trang
        var pagedStats = orderedStats
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // 2. Lấy cache Building & VehicleType để map Name nhanh trong bộ nhớ
        var buildings = (await _buildingRepository.GetAllAsync()).ToDictionary(b => b.Id, b => b.Name);
        var vehicleTypes = (await _vehicleTypeRepository.GetAllAsync()).ToDictionary(vt => vt.Id, vt => vt.TypeName);

        var dtos = pagedStats.Select(s => new RevenueStatisticDto
        {
            Id = s.Id,
            BuildingId = s.BuildingId,
            BuildingName = buildings.TryGetValue(s.BuildingId, out var bName) ? bName : "Unknown Building",
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            PeriodType = s.PeriodType,
            VehicleTypeId = s.VehicleTypeId,
            VehicleTypeName = s.VehicleTypeId.HasValue && vehicleTypes.TryGetValue(s.VehicleTypeId.Value, out var vtName) 
                ? vtName 
                : "Total Revenue",
            TotalRevenue = s.TotalRevenue,
            TotalBookings = s.TotalBookings,
            TotalSessions = s.TotalSessions,
            TotalSubscriptions = s.TotalSubscriptions
        }).ToList();

        return PagedResult<RevenueStatisticDto>.Create(dtos, totalCount, pageIndex, pageSize);
    }

    /// <summary>
    /// Lấy chi tiết thống kê doanh thu kèm danh sách giao dịch.
    /// </summary>
    public async Task<RevenueStatisticDto> GetRevenueStatisticByIdAsync(int id)
    {
        var s = await _revenueStatisticRepository.GetByIdAsync(id);
        if (s == null)
        {
            throw new KeyNotFoundException($"Revenue statistic with ID {id} not found.");
        }

        var building = await _buildingRepository.GetByIdAsync(s.BuildingId);
        string buildingName = building?.Name ?? "Unknown Building";

        string vehicleTypeName = "Total Revenue";
        if (s.VehicleTypeId.HasValue)
        {
            var vt = await _vehicleTypeRepository.GetByIdAsync(s.VehicleTypeId.Value);
            vehicleTypeName = vt?.TypeName ?? "Unknown Type";
        }

        var dto = new RevenueStatisticDto
        {
            Id = s.Id,
            BuildingId = s.BuildingId,
            BuildingName = buildingName,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            PeriodType = s.PeriodType,
            VehicleTypeId = s.VehicleTypeId,
            VehicleTypeName = vehicleTypeName,
            TotalRevenue = s.TotalRevenue,
            TotalBookings = s.TotalBookings,
            TotalSessions = s.TotalSessions,
            TotalSubscriptions = s.TotalSubscriptions,
            Payments = new List<RevenuePaymentDetailDto>()
        };

        // Lấy danh sách Payment liên kết
        var links = await _revenueStatisticPaymentRepository.FindAsync(lsp => lsp.StatisticId == id);
        foreach (var link in links)
        {
            var p = await _paymentRepository.GetByIdAsync(link.PaymentId);
            if (p == null) continue;

            string sourceType = "Unknown";
            string? licensePlate = null;

            if (p.SessionId.HasValue)
            {
                sourceType = "Session";
                var session = await _sessionRepository.GetByIdAsync(p.SessionId.Value);
                licensePlate = session?.LicensePlateIn;
            }
            else if (p.BookingId.HasValue)
            {
                sourceType = "Booking";
                var booking = await _bookingRepository.GetByIdAsync(p.BookingId.Value);
                var vehicle = booking != null ? await _vehicleRepository.GetByIdAsync(booking.VehicleId) : null;
                licensePlate = vehicle?.LicensePlate;
            }
            else if (p.MonthlySubscriptionId.HasValue)
            {
                sourceType = "MonthlySubscription";
                var sub = await _subscriptionRepository.GetByIdAsync(p.MonthlySubscriptionId.Value);
                var vehicle = sub != null ? await _vehicleRepository.GetByIdAsync(sub.VehicleId) : null;
                licensePlate = vehicle?.LicensePlate;
            }

            dto.Payments.Add(new RevenuePaymentDetailDto
            {
                PaymentId = p.Id,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentTime = p.PaymentTime,
                SourceType = sourceType,
                LicensePlate = licensePlate
            });
        }

        return dto;
    }

    /// <summary>
    /// Trigger tính toán lại doanh thu của ngày đó sau khi có một giao dịch PAID.
    /// </summary>
    public async Task UpdateRevenueAfterPaymentAsync(int paymentId)
    {
        var targetPayment = await _paymentRepository.GetByIdAsync(paymentId);
        if (targetPayment == null || targetPayment.PaymentStatus != "PAID") return;

        // 1. Xác định ngày thanh toán (DateOnly)
        var paymentDate = DateOnly.FromDateTime(targetPayment.PaymentTime ?? targetPayment.CreatedAt);

        // 2. Xác định BuildingId và VehicleTypeId của giao dịch này
        int buildingId = 0;
        int? vehicleTypeId = null;

        if (targetPayment.SessionId.HasValue)
        {
            var session = await _sessionRepository.GetByIdAsync(targetPayment.SessionId.Value);
            if (session != null)
            {
                buildingId = session.BuildingId;
                var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId);
                vehicleTypeId = vehicle?.VehicleTypeId;
            }
        }
        else if (targetPayment.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(targetPayment.BookingId.Value);
            if (booking != null)
            {
                buildingId = booking.BuildingId;
                vehicleTypeId = booking.VehicleTypeId;
            }
        }
        else if (targetPayment.MonthlySubscriptionId.HasValue)
        {
            var sub = await _subscriptionRepository.GetByIdAsync(targetPayment.MonthlySubscriptionId.Value);
            if (sub != null)
            {
                buildingId = sub.BuildingId;
                var vehicle = await _vehicleRepository.GetByIdAsync(sub.VehicleId);
                vehicleTypeId = vehicle?.VehicleTypeId;
            }
        }

        if (buildingId == 0) return; // Không tìm thấy thông tin tòa nhà liên quan

        // 3. Lấy tất cả các giao dịch thanh toán thành công (PAID) trong ngày hôm đó
        var allPayments = await _paymentRepository.FindAsync(p =>
            p.PaymentStatus == "PAID" &&
            p.PaymentTime.HasValue &&
            DateOnly.FromDateTime(p.PaymentTime.Value) == paymentDate
        );

        // Phân loại và phân tích thông tin của từng payment trong bộ nhớ
        var paymentsWithInfo = new List<(PBMS.Domain.Entities.Payment Payment, int BuildingId, int? VehicleTypeId)>();
        foreach (var p in allPayments)
        {
            int bId = 0;
            int? vTypeId = null;

            if (p.SessionId.HasValue)
            {
                var s = await _sessionRepository.GetByIdAsync(p.SessionId.Value);
                if (s != null)
                {
                    bId = s.BuildingId;
                    var v = await _vehicleRepository.GetByIdAsync(s.VehicleId);
                    vTypeId = v?.VehicleTypeId;
                }
            }
            else if (p.BookingId.HasValue)
            {
                var b = await _bookingRepository.GetByIdAsync(p.BookingId.Value);
                if (b != null)
                {
                    bId = b.BuildingId;
                    vTypeId = b.VehicleTypeId;
                }
            }
            else if (p.MonthlySubscriptionId.HasValue)
            {
                var sub = await _subscriptionRepository.GetByIdAsync(p.MonthlySubscriptionId.Value);
                if (sub != null)
                {
                    bId = sub.BuildingId;
                    var v = await _vehicleRepository.GetByIdAsync(sub.VehicleId);
                    vTypeId = v?.VehicleTypeId;
                }
            }

            if (bId == buildingId)
            {
                paymentsWithInfo.Add((p, bId, vTypeId));
            }
        }

        // 4. Tiến hành tính toán và lưu trữ/cập nhật hai dòng thống kê song song
        // Dòng A: Thống kê chi tiết theo loại xe cụ thể
        if (vehicleTypeId.HasValue)
        {
            var vehicleTypePayments = paymentsWithInfo.Where(x => x.VehicleTypeId == vehicleTypeId).ToList();
            await SaveOrUpdateStatisticGroupAsync(buildingId, paymentDate, vehicleTypeId.Value, vehicleTypePayments);
        }

        // Dòng B: Thống kê doanh thu tổng hợp toàn bộ bãi (vehicle_type_id = null)
        await SaveOrUpdateStatisticGroupAsync(buildingId, paymentDate, null, paymentsWithInfo);
    }

    /// <summary>
    /// Hàm helper để cập nhật/lưu trữ bản ghi RevenueStatistic và liên kết Payment.
    /// </summary>
    private async Task SaveOrUpdateStatisticGroupAsync(
        int buildingId, 
        DateOnly date, 
        int? vehicleTypeId, 
        List<(PBMS.Domain.Entities.Payment Payment, int BuildingId, int? VehicleTypeId)> groupPayments)
    {
        // Kiểm tra xem đã có dòng thống kê DAILY cho ngày và nhóm xe này chưa
        var stat = (await _revenueStatisticRepository.FindAsync(rs =>
            rs.BuildingId == buildingId &&
            rs.StartDate == date &&
            rs.EndDate == date &&
            rs.PeriodType == "DAILY" &&
            rs.VehicleTypeId == vehicleTypeId
        )).FirstOrDefault();

        decimal totalRevenue = groupPayments.Sum(x => x.Payment.Amount);
        int totalBookings = groupPayments.Count(x => x.Payment.BookingId.HasValue);
        int totalSessions = groupPayments.Count(x => x.Payment.SessionId.HasValue);
        int totalSubs = groupPayments.Count(x => x.Payment.MonthlySubscriptionId.HasValue);

        if (stat == null)
        {
            stat = new PBMS.Domain.Entities.RevenueStatistic
            {
                BuildingId = buildingId,
                StartDate = date,
                EndDate = date,
                PeriodType = "DAILY",
                VehicleTypeId = vehicleTypeId,
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                TotalSessions = totalSessions,
                TotalSubscriptions = totalSubs
            };
            await _revenueStatisticRepository.AddAsync(stat);
        }
        else
        {
            stat.TotalRevenue = totalRevenue;
            stat.TotalBookings = totalBookings;
            stat.TotalSessions = totalSessions;
            stat.TotalSubscriptions = totalSubs;
            _revenueStatisticRepository.Update(stat);
        }

        // Lưu bản ghi thống kê để lấy ID
        await _revenueStatisticRepository.SaveChangesAsync();

        // Xóa liên kết cũ trong bảng trung gian và cập nhật liên kết mới
        var oldLinks = await _revenueStatisticPaymentRepository.FindAsync(lsp => lsp.StatisticId == stat.Id);
        await _revenueStatisticPaymentRepository.RemoveRangeAsync(oldLinks);
        await _revenueStatisticPaymentRepository.SaveChangesAsync();

        var newLinks = groupPayments.Select(x => new PBMS.Domain.Entities.RevenueStatisticPayment
        {
            StatisticId = stat.Id,
            PaymentId = x.Payment.Id
        }).ToList();

        await _revenueStatisticPaymentRepository.AddRangeAsync(newLinks);
        await _revenueStatisticPaymentRepository.SaveChangesAsync();
    }
}
