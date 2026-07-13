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
/// Triển khai dịch vụ thống kê doanh thu thời gian thực (Real-time).
/// </summary>
public class RevenueService : IRevenueService
{
    private readonly IRepository<PBMS.Domain.Entities.RevenueStatistic> _revenueStatisticRepository;
    private readonly IRepository<PBMS.Domain.Entities.RevenueStatisticPayment> _revenueStatisticPaymentRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRepository<PBMS.Domain.Entities.Building> _buildingRepository;
    private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepository;
    private readonly IRepository<PBMS.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<PBMS.Domain.Entities.Vehicle> _vehicleRepository;
    private readonly IRepository<PBMS.Domain.Entities.VehicleType> _vehicleTypeRepository;

    public RevenueService(
        IRepository<PBMS.Domain.Entities.RevenueStatistic> revenueStatisticRepository,
        IRepository<PBMS.Domain.Entities.RevenueStatisticPayment> revenueStatisticPaymentRepository,
        IPaymentRepository paymentRepository,
        IRepository<PBMS.Domain.Entities.Building> buildingRepository,
        IRepository<PBMS.Domain.Entities.ParkingSession> sessionRepository,
        IRepository<PBMS.Domain.Entities.Booking> bookingRepository,
        IRepository<PBMS.Domain.Entities.Vehicle> vehicleRepository,
        IRepository<PBMS.Domain.Entities.VehicleType> vehicleTypeRepository)
    {
        _revenueStatisticRepository = revenueStatisticRepository;
        _revenueStatisticPaymentRepository = revenueStatisticPaymentRepository;
        _paymentRepository = paymentRepository;
        _buildingRepository = buildingRepository;
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
    }

    /// <summary>
    /// Lấy danh sách thống kê doanh thu phân trang.
    /// Tính toán động thời gian thực từ bảng Payment.
    /// </summary>
    public async Task<PagedResult<RevenueStatisticDto>> GetRevenueStatisticsAsync(RevenueFilterDto filter, int pageIndex, int pageSize)
    {
        // 1. Xác định khoảng thời gian UTC dựa theo múi giờ Việt Nam (UTC+7) của filter
        DateTime? fromDateUtc = filter.StartDate.HasValue 
            ? filter.StartDate.Value.ToDateTime(TimeOnly.MinValue).AddHours(-7) 
            : null;

        DateTime? toDateUtc = filter.EndDate.HasValue 
            ? filter.EndDate.Value.ToDateTime(TimeOnly.MaxValue).AddHours(-7) 
            : null;

        // 2. Lấy tất cả các giao dịch PAID trong khoảng thời gian
        var allPayments = await _paymentRepository.GetPaidPaymentsAsync(fromDateUtc, toDateUtc);

        // 3. Lấy cache Building & VehicleType để map Name nhanh trong bộ nhớ
        var buildings = (await _buildingRepository.GetAllAsync()).ToDictionary(b => b.Id, b => b.Name);
        var vehicleTypes = (await _vehicleTypeRepository.GetAllAsync()).ToDictionary(vt => vt.Id, vt => vt.TypeName);

        // 4. Xử lý thông tin và gom nhóm theo ngày/tháng/năm trong bộ nhớ theo giờ Việt Nam
        var processedPayments = new List<ProcessedPayment>();
        foreach (var p in allPayments)
        {
            int? bId = null;
            int? vTypeId = null;

            if (p.SessionId.HasValue && p.Session != null)
            {
                bId = p.Session.BuildingId;
                vTypeId = p.Session.Vehicle?.VehicleTypeId;
            }
            else if (p.BookingId.HasValue && p.Booking != null)
            {
                bId = p.Booking.BuildingId;
                vTypeId = p.Booking.VehicleTypeId;
            }

            if (bId == null) continue;
            if (filter.BuildingId.HasValue && bId.Value != filter.BuildingId.Value) continue;

            // Đổi từ UTC sang giờ Việt Nam (UTC+7)
            var localTime = (p.PaymentTime ?? p.CreatedAt).AddHours(7);
            var localDate = DateOnly.FromDateTime(localTime);

            DateOnly startDate, endDate;
            if (filter.PeriodType == "MONTHLY")
            {
                startDate = new DateOnly(localDate.Year, localDate.Month, 1);
                endDate = new DateOnly(localDate.Year, localDate.Month, DateTime.DaysInMonth(localDate.Year, localDate.Month));
            }
            else if (filter.PeriodType == "YEARLY")
            {
                startDate = new DateOnly(localDate.Year, 1, 1);
                endDate = new DateOnly(localDate.Year, 12, 31);
            }
            else // DAILY
            {
                startDate = localDate;
                endDate = localDate;
            }

            // Lọc theo ngày local (nếu bộ lọc có giới hạn)
            if (filter.StartDate.HasValue && startDate < filter.StartDate.Value) continue;
            if (filter.EndDate.HasValue && endDate > filter.EndDate.Value) continue;

            processedPayments.Add(new ProcessedPayment
            {
                Payment = p,
                BuildingId = bId.Value,
                VehicleTypeId = vTypeId,
                StartDate = startDate,
                EndDate = endDate
            });
        }

        // 5. Gom nhóm theo BuildingId, StartDate, EndDate
        var groupedByPeriod = processedPayments
            .GroupBy(p => new { p.BuildingId, p.StartDate, p.EndDate })
            .ToList();

        var statsList = new List<RevenueStatisticDto>();

        foreach (var group in groupedByPeriod)
        {
            var bId = group.Key.BuildingId;
            var sDate = group.Key.StartDate;
            var eDate = group.Key.EndDate;
            string bName = buildings.TryGetValue(bId, out var name) ? name : "Unknown Building";

            var paymentsInGroup = group.ToList();

            // Nhóm con: Theo từng loại xe (VehicleTypeId)
            var byVehicleType = paymentsInGroup
                .GroupBy(p => p.VehicleTypeId)
                .ToList();

            foreach (var vtGroup in byVehicleType)
            {
                if (vtGroup.Key.HasValue)
                {
                    int vtId = vtGroup.Key.Value;
                    string vtName = vehicleTypes.TryGetValue(vtId, out var typeName) ? typeName : "Unknown Type";
                    var vtPayments = vtGroup.Select(x => x.Payment).ToList();

                    statsList.Add(new RevenueStatisticDto
                    {
                        Id = EncodeId(bId, sDate, vtId, filter.PeriodType),
                        BuildingId = bId,
                        BuildingName = bName,
                        StartDate = sDate,
                        EndDate = eDate,
                        PeriodType = filter.PeriodType,
                        VehicleTypeId = vtId,
                        VehicleTypeName = vtName,
                        TotalRevenue = vtPayments.Sum(p => p.Amount),
                        TotalBookings = vtPayments.Count(p => p.BookingId.HasValue),
                        TotalSessions = vtPayments.Count(p => p.SessionId.HasValue),
                        TotalSubscriptions = 0
                    });
                }
            }

            // Dòng tổng hợp (VehicleTypeId = null)
            var allGroupPayments = paymentsInGroup.Select(x => x.Payment).ToList();
            statsList.Add(new RevenueStatisticDto
            {
                Id = EncodeId(bId, sDate, null, filter.PeriodType),
                BuildingId = bId,
                BuildingName = bName,
                StartDate = sDate,
                EndDate = eDate,
                PeriodType = filter.PeriodType,
                VehicleTypeId = null,
                VehicleTypeName = "Total Revenue",
                TotalRevenue = allGroupPayments.Sum(p => p.Amount),
                TotalBookings = allGroupPayments.Count(p => p.BookingId.HasValue),
                TotalSessions = allGroupPayments.Count(p => p.SessionId.HasValue),
                TotalSubscriptions = 0
            });
        }

        var orderedStats = statsList.OrderByDescending(rs => rs.StartDate).ToList();
        var totalCount = orderedStats.Count;

        var pagedStats = orderedStats
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<RevenueStatisticDto>.Create(pagedStats, totalCount, pageIndex, pageSize);
    }

    /// <summary>
    /// Lấy chi tiết thống kê doanh thu kèm danh sách giao dịch chi tiết.
    /// Giải mã ID tổng hợp thành các tham số truy vấn gốc.
    /// </summary>
    public async Task<RevenueStatisticDto> GetRevenueStatisticByIdAsync(int id)
    {
        // 1. Giải mã ID tổng hợp
        var (buildingId, vehicleTypeId, periodType, startDate) = DecodeId(id);

        // 2. Xác định ngày kết thúc của chu kỳ
        DateOnly endDate;
        if (periodType == "MONTHLY")
        {
            endDate = new DateOnly(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));
        }
        else if (periodType == "YEARLY")
        {
            endDate = new DateOnly(startDate.Year, 12, 31);
        }
        else
        {
            endDate = startDate;
        }

        // 3. Quy đổi ngày local (Vietnam UTC+7) sang UTC range để query DB tối ưu
        DateTime fromDateUtc = startDate.ToDateTime(TimeOnly.MinValue).AddHours(-7);
        DateTime toDateUtc = endDate.ToDateTime(TimeOnly.MaxValue).AddHours(-7);

        // 4. Lấy danh sách thanh toán trong khoảng thời gian tương ứng
        var allPayments = await _paymentRepository.GetPaidPaymentsAsync(fromDateUtc, toDateUtc);

        // 5. Lấy tên Tòa nhà & Loại xe
        var building = await _buildingRepository.GetByIdAsync(buildingId);
        string buildingName = building?.Name ?? "Unknown Building";

        string vehicleTypeName = "Total Revenue";
        if (vehicleTypeId.HasValue)
        {
            var vt = await _vehicleTypeRepository.GetByIdAsync(vehicleTypeId.Value);
            vehicleTypeName = vt?.TypeName ?? "Unknown Type";
        }

        // 6. Lọc và lấy các payment hợp lệ
        var filteredPayments = new List<PBMS.Domain.Entities.Payment>();
        foreach (var p in allPayments)
        {
            int? pBuildingId = null;
            int? pVehicleTypeId = null;

            if (p.SessionId.HasValue && p.Session != null)
            {
                pBuildingId = p.Session.BuildingId;
                pVehicleTypeId = p.Session.Vehicle?.VehicleTypeId;
            }
            else if (p.BookingId.HasValue && p.Booking != null)
            {
                pBuildingId = p.Booking.BuildingId;
                pVehicleTypeId = p.Booking.VehicleTypeId;
            }

            if (pBuildingId == null || pBuildingId.Value != buildingId) continue;

            // Kiểm tra khớp múi giờ Việt Nam
            var localTime = (p.PaymentTime ?? p.CreatedAt).AddHours(7);
            var localDate = DateOnly.FromDateTime(localTime);
            if (localDate < startDate || localDate > endDate) continue;

            // Nếu lọc loại xe cụ thể
            if (vehicleTypeId.HasValue && pVehicleTypeId != vehicleTypeId.Value) continue;

            filteredPayments.Add(p);
        }

        // 7. Tạo DTO kết quả
        var dto = new RevenueStatisticDto
        {
            Id = id,
            BuildingId = buildingId,
            BuildingName = buildingName,
            StartDate = startDate,
            EndDate = endDate,
            PeriodType = periodType,
            VehicleTypeId = vehicleTypeId,
            VehicleTypeName = vehicleTypeName,
            TotalRevenue = filteredPayments.Sum(p => p.Amount),
            TotalBookings = filteredPayments.Count(p => p.BookingId.HasValue),
            TotalSessions = filteredPayments.Count(p => p.SessionId.HasValue),
            TotalSubscriptions = 0,
            Payments = new List<RevenuePaymentDetailDto>()
        };

        // 8. Đổ chi tiết từng giao dịch
        foreach (var p in filteredPayments)
        {
            string sourceType = "Unknown";
            string? licensePlate = null;

            if (p.SessionId.HasValue)
            {
                sourceType = "Session";
                var session = p.Session;
                licensePlate = session?.LicensePlateIn;
            }
            else if (p.BookingId.HasValue)
            {
                sourceType = "Booking";
                var booking = p.Booking;
                var vehicle = booking != null ? await _vehicleRepository.GetByIdAsync(booking.VehicleId) : null;
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
    /// Vì tính toán doanh thu đã chuyển sang thời gian thực trên mỗi yêu cầu,
    /// phương thức trigger này được giữ lại để tương thích ngược nhưng không thực hiện ghi DB nữa.
    /// </summary>
    public Task UpdateRevenueAfterPaymentAsync(int paymentId)
    {
        return Task.CompletedTask;
    }

    #region Helper Methods (Mã hóa và giải mã ID tổng hợp)

    private static int EncodeId(int buildingId, DateOnly startDate, int? vehicleTypeId, string periodType)
    {
        // 9 bits cho BuildingId (0-511)
        int bId = buildingId & 0x1FF;
        // 5 bits cho VehicleTypeId (0-31, 0 là null)
        int vId = (vehicleTypeId ?? 0) & 0x1F;
        // 2 bits cho PeriodType (0: DAILY, 1: MONTHLY, 2: YEARLY)
        int pType = periodType switch
        {
            "DAILY" => 0,
            "MONTHLY" => 1,
            "YEARLY" => 2,
            _ => 0
        };
        // 6 bits cho YearOffset (0-63, từ năm 2024 đến 2087)
        int yearOffset = Math.Max(0, startDate.Year - 2024) & 0x3F;
        // 4 bits cho Month (1-12)
        int month = startDate.Month & 0x0F;
        // 5 bits cho Day (1-31)
        int day = startDate.Day & 0x1F;

        int encoded = 0;
        encoded |= (bId & 0x1FF) << 22;
        encoded |= (vId & 0x1F) << 17;
        encoded |= (pType & 0x03) << 15;
        encoded |= (yearOffset & 0x3F) << 9;
        encoded |= (month & 0x0F) << 5;
        encoded |= (day & 0x1F);

        return encoded;
    }

    private static (int BuildingId, int? VehicleTypeId, string PeriodType, DateOnly StartDate) DecodeId(int id)
    {
        int bId = (id >> 22) & 0x1FF;
        int vId = (id >> 17) & 0x1F;
        int pTypeVal = (id >> 15) & 0x03;
        int yearOffset = (id >> 9) & 0x3F;
        int month = (id >> 5) & 0x0F;
        int day = id & 0x1F;

        int? vehicleTypeId = vId == 0 ? null : vId;
        string periodType = pTypeVal switch
        {
            0 => "DAILY",
            1 => "MONTHLY",
            2 => "YEARLY",
            _ => "DAILY"
        };

        int year = 2024 + yearOffset;
        DateOnly startDate;
        try
        {
            startDate = new DateOnly(year, month, day);
        }
        catch
        {
            startDate = new DateOnly(2024, 1, 1);
        }

        return (bId, vehicleTypeId, periodType, startDate);
    }

    #endregion

    private class ProcessedPayment
    {
        public PBMS.Domain.Entities.Payment Payment { get; set; } = null!;
        public int BuildingId { get; set; }
        public int? VehicleTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
