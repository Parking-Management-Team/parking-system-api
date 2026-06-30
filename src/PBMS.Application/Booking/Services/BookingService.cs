using PBMS.Application.Booking.DTOs;
using PBMS.Application.Booking.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Domain.Enums;
using PBMS.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using BookingEntity = PBMS.Domain.Entities.Booking;
using ParkingSlotEntity = PBMS.Domain.Entities.ParkingSlot;
using VehicleEntity = PBMS.Domain.Entities.Vehicle;
using VehicleTypeEntity = PBMS.Domain.Entities.VehicleType;
using BuildingEntity = PBMS.Domain.Entities.Building;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;
using PaymentEntity = PBMS.Domain.Entities.Payment;

namespace PBMS.Application.Booking.Services;

/// <summary>
/// Triển khai nghiệp vụ quản lý Đặt chỗ trước (IBookingService).
///
/// Nguyên tắc: Service KHÔNG biết về EF Core hay Database.
///             Chỉ giao tiếp với DB thông qua các Repository Interface.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly IRepository<VehicleTypeEntity> _vehicleTypeRepository;
    private readonly IRepository<BuildingEntity> _buildingRepository;
    private readonly IBuildingRepository _buildingDetailRepository;
    private readonly IPricingPolicyRepository _pricingPolicyRepository;
    private readonly IRepository<ParkingSessionEntity> _sessionRepository;
    private readonly IRepository<ParkingSlotEntity> _parkingSlotRepository;
    private readonly IRepository<PaymentEntity> _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IBlacklistRepository _blacklistRepository;

    // Cấu hình nghiệp vụ (lấy từ Configuration hoặc mặc định)
    private int MinBookingMinutes => int.TryParse(_configuration["BookingSettings:MinBookingMinutes"], out var val) ? val : 15;
    private int MinBookingDurationHours => int.TryParse(_configuration["BookingSettings:MinBookingDurationHours"], out var val) ? val : 4;
    private int PaymentDeadlineMinutes => int.TryParse(_configuration["BookingSettings:PaymentDeadlineMinutes"], out var val) ? val : 15;
    private int CheckinGracePeriodMinutes => int.TryParse(_configuration["BookingSettings:CheckinGracePeriodMinutes"], out var val) ? val : 30;

    // Múi giờ Việt Nam (UTC+7)
    private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    /// <summary>
    /// Chuyển đổi DateTime UTC sang DateTimeOffset giờ Việt Nam (UTC+7).
    /// Dùng cho pricing lookup — không gửi cho PostgreSQL.
    /// </summary>
    private static DateTimeOffset ToVietnamTimeOffset(DateTime utcDateTime)
    {
        return new DateTimeOffset(utcDateTime, TimeSpan.Zero).ToOffset(TimeSpan.FromHours(7));
    }

    /// <summary>
    /// Lấy phần ngày (Date) theo giờ Việt Nam từ DateTime UTC.
    /// </summary>
    private static DateTime GetVietnamDate(DateTime utcDateTime)
    {
        var vnOffset = ToVietnamTimeOffset(utcDateTime);
        return vnOffset.Date;
    }

    /// <summary>
    /// Constructor nhận các dependency qua Dependency Injection.
    /// </summary>
    public BookingService(
        IBookingRepository bookingRepository,
        IRepository<VehicleEntity> _vehicleRepositoryMock,
        IRepository<VehicleTypeEntity> _vehicleTypeRepositoryMock,
        IRepository<BuildingEntity> _buildingRepositoryMock,
        IBuildingRepository _buildingDetailRepositoryMock,
        IPricingPolicyRepository _pricingPolicyRepositoryMock,
        IRepository<ParkingSessionEntity> _sessionRepositoryMock,
        IRepository<ParkingSlotEntity> parkingSlotRepository,
        IRepository<PaymentEntity> paymentRepositoryMock,
        IUnitOfWork _unitOfWorkMock,
        IConfiguration configuration,
        IBlacklistRepository blacklistRepository)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _vehicleRepository = _vehicleRepositoryMock ?? throw new ArgumentNullException(nameof(_vehicleRepositoryMock));
        _vehicleTypeRepository = _vehicleTypeRepositoryMock ?? throw new ArgumentNullException(nameof(_vehicleTypeRepositoryMock));
        _buildingRepository = _buildingRepositoryMock ?? throw new ArgumentNullException(nameof(_buildingRepositoryMock));
        _buildingDetailRepository = _buildingDetailRepositoryMock ?? throw new ArgumentNullException(nameof(_buildingDetailRepositoryMock));
        _pricingPolicyRepository = _pricingPolicyRepositoryMock ?? throw new ArgumentNullException(nameof(_pricingPolicyRepositoryMock));
        _sessionRepository = _sessionRepositoryMock ?? throw new ArgumentNullException(nameof(_sessionRepositoryMock));
        _parkingSlotRepository = parkingSlotRepository ?? throw new ArgumentNullException(nameof(parkingSlotRepository));
        _paymentRepository = paymentRepositoryMock ?? throw new ArgumentNullException(nameof(paymentRepositoryMock));
        _unitOfWork = _unitOfWorkMock ?? throw new ArgumentNullException(nameof(_unitOfWorkMock));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _blacklistRepository = blacklistRepository ?? throw new ArgumentNullException(nameof(blacklistRepository));
    }

    // -----------------------------------------------------------------------
    // CREATE
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tạo Booking mới (trạng thái PENDING).
    ///
    /// Luồng xử lý:
    ///   1. Validate thời gian PlannedCheckinTime (1h - 8h từ Now)
    ///   2. Tra cứu Vehicle theo biển số
    ///   3. Kiểm tra Building tồn tại
    ///   4. Kiểm tra General Capacity còn lại tại Building cho loại xe
    ///   5. Tính Deposit Fee (BasePrice của PricingWindow tại giờ check-in dự kiến)
    ///   6. Tạo entity Booking và lưu vào DB
    /// </summary>
    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
    {
        var now = DateTime.UtcNow;

        // Đảm bảo DateTime luôn là UTC cho PostgreSQL
        var plannedCheckinUtc = request.PlannedCheckinTime.Kind == DateTimeKind.Utc
            ? request.PlannedCheckinTime
            : request.PlannedCheckinTime.ToUniversalTime();
        var plannedCheckoutUtc = request.PlannedCheckoutTime.HasValue
            ? (request.PlannedCheckoutTime.Value.Kind == DateTimeKind.Utc
                ? request.PlannedCheckoutTime.Value
                : request.PlannedCheckoutTime.Value.ToUniversalTime())
            : (DateTime?)null;

        // Bước 1: Validate thời gian đặt chỗ
        // Check-in phải cách hiện tại tối thiểu 15 phút (không giới hạn tối đa)
        var minAllowed = now.AddMinutes(MinBookingMinutes);

        if (plannedCheckinUtc < minAllowed)
        {
            // Hiển thị giờ VN (UTC+7) cho user-friendly
            var minVn = ToVietnamTimeOffset(minAllowed);
            throw new DomainException(
                errorCode: "INVALID_BOOKING_TIME",
                message: $"Thời gian đặt chỗ phải cách hiện tại ít nhất {MinBookingMinutes} phút. " +
                         $"Thời gian hợp lệ: từ [{minVn:yyyy-MM-dd HH:mm}] trở đi (Giờ VN)."
            );
        }

        // Bước 2: Tra cứu Vehicle theo biển số
        var normalizedPlate = request.LicensePlate.Trim().ToUpper();
        var vehicles = await _vehicleRepository.FindAsync(v => v.LicensePlate.ToUpper() == normalizedPlate);
        var vehicle = vehicles.FirstOrDefault();

        if (vehicle == null)
        {
            throw new DomainException(
                errorCode: "VEHICLE_NOT_FOUND",
                message: $"Không tìm thấy xe với biển số '{request.LicensePlate}' trong hệ thống."
            );
        }

        // Kiểm tra xem phương tiện có nằm trong danh sách đen (Blacklist) không
        var isVehicleBlacklisted = await _blacklistRepository.AnyAsync(b => b.VehicleId == vehicle.Id && !b.IsDeleted);
        if (isVehicleBlacklisted)
        {
            throw new DomainException(
                errorCode: "VEHICLE_BLACKLISTED",
                message: $"Phương tiện '{vehicle.LicensePlate}' đang nằm trong danh sách đen. Không thể đặt chỗ."
            );
        }

        // Kiểm tra xem tài khoản đặt chỗ có nằm trong danh sách đen không (thông qua các phương tiện sở hữu)
        var isAccountBlacklisted = await _blacklistRepository.AnyAsync(b => b.Vehicle != null && b.Vehicle.AccountId == request.AccountId && !b.IsDeleted);
        if (isAccountBlacklisted)
        {
            throw new DomainException(
                errorCode: "ACCOUNT_BLACKLISTED",
                message: "Tài khoản của bạn có phương tiện nằm trong danh sách đen. Không thể thực hiện đặt chỗ."
            );
        }

        // Bước 3: Kiểm tra Building tồn tại
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId);
        if (building == null)
        {
            throw new DomainException(
                errorCode: "BUILDING_NOT_FOUND",
                message: $"Tòa nhà với ID {request.BuildingId} không tồn tại."
            );
        }

        // Bước 4: Kiểm tra General Capacity còn lại
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(vehicle.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new DomainException(
                errorCode: "VEHICLE_TYPE_NOT_FOUND",
                message: "Không tìm thấy thông tin loại xe."
            );
        }

        // Tổng chỗ General = Tổng capacity Zone General của loại xe
        var totalCapacity = await _buildingDetailRepository.GetTotalGeneralCapacityAsync(
            request.BuildingId, vehicle.VehicleTypeId);

        // Tính toán Sức chứa hiệu dụng sau khi trừ đi chỗ đỗ dự phòng (Buffer Slots)
        var bufferSlots = (int)Math.Ceiling(totalCapacity * (vehicleType.BufferRatio / 100.0));
        var effectiveCapacity = totalCapacity - bufferSlots;

        // Đã sử dụng = Active ParkingSession + Active Booking (Pending | Confirmed)
        var activeSessions = await _sessionRepository.CountAsync(s =>
            s.BuildingId == request.BuildingId &&
            s.Vehicle.VehicleTypeId == vehicle.VehicleTypeId &&
            s.SessionStatus == SessionStatus.Active);

        var start = plannedCheckinUtc;
        var end = plannedCheckoutUtc ?? plannedCheckinUtc.AddHours(MinBookingDurationHours);

        if (end <= start)
        {
            throw new DomainException(
                errorCode: "INVALID_BOOKING_TIME",
                message: "Thời gian dự kiến ra bãi phải lớn hơn thời gian dự kiến vào bãi."
            );
        }

        // Kiểm tra thời lượng đặt chỗ tối thiểu
        var duration = end - start;
        if (duration.TotalHours < MinBookingDurationHours)
        {
            throw new DomainException(
                errorCode: "INVALID_BOOKING_DURATION",
                message: $"Thời lượng đặt chỗ tối thiểu là {MinBookingDurationHours} tiếng. " +
                         $"Thời lượng hiện tại: {duration.TotalMinutes:F0} phút."
            );
        }

        var activeBookings = await _bookingRepository.GetActiveBookingsCountAsync(
            request.BuildingId, vehicle.VehicleTypeId, start, end);

        var usedCapacity = activeSessions + activeBookings;

        if (usedCapacity >= effectiveCapacity)
        {
            throw new DomainException(
                errorCode: "NO_CAPACITY",
                message: $"Tòa nhà không còn chỗ trống cho loại xe này (Đã trừ {vehicleType.BufferRatio}% chỗ dự phòng). " +
                         $"Tổng sức chứa: {totalCapacity}, Chỗ khả dụng: {effectiveCapacity}, Đang sử dụng: {usedCapacity}."
            );
        }

        // Bước 5: Tính Deposit Fee (bằng đúng tổng số tiền tạm tính cho toàn bộ thời gian đặt chỗ)
        // Chuyển sang giờ VN để lookup pricing policy và pricing window đúng ngày
        var checkinVnOffset = ToVietnamTimeOffset(plannedCheckinUtc);
        var checkinVnDate = checkinVnOffset.Date;
        var pricingPolicy = await _pricingPolicyRepository.GetActivePolicyAsync(
            vehicle.VehicleTypeId, checkinVnDate);

        if (pricingPolicy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: "Không tìm thấy chính sách giá phí áp dụng cho loại xe tại thời điểm đặt chỗ."
            );
        }

        // Tìm PricingWindow tương ứng với giờ check-in dự kiến (dùng giờ VN)
        var checkInTimeOfDay = checkinVnOffset.TimeOfDay;
        var applicableWindow = pricingPolicy.PricingWindows
            .FirstOrDefault(w => IsTimeInWindow(checkInTimeOfDay, w.StartTime, w.EndTime));

        if (applicableWindow == null)
        {
            // Fallback: lấy window đầu tiên nếu không tìm được window khớp
            applicableWindow = pricingPolicy.PricingWindows.FirstOrDefault();
        }

        if (applicableWindow == null)
        {
            throw new DomainException(
                errorCode: "PRICING_WINDOW_NOT_FOUND",
                message: "Không tìm thấy khung giờ tính giá phù hợp."
            );
        }

        decimal depositAmount = applicableWindow.BasePrice;

        if (request.SlotId.HasValue)
        {
            // 1. Chỉ áp dụng chọn slot cho xe ô tô
            var isCar = !string.IsNullOrWhiteSpace(vehicleType.TypeName) && (
                string.Equals(vehicleType.TypeName, "Car", StringComparison.OrdinalIgnoreCase) ||
                vehicleType.TypeName.Contains("CAR", StringComparison.OrdinalIgnoreCase) ||
                vehicleType.TypeName.Contains("AUTO", StringComparison.OrdinalIgnoreCase));
            if (!isCar)
            {
                throw new DomainException(
                    errorCode: "INVALID_SLOT_SELECTION",
                    message: "Chỉ cho phép chọn vị trí đỗ (Slot) đối với xe ô tô."
                );
            }

            // 2. Kiểm tra slot tồn tại và thuộc tòa nhà đã chọn
            var slot = await _parkingSlotRepository.FirstOrDefaultAsync(s => 
                s.Id == request.SlotId.Value && 
                s.Zone.Floor.BuildingId == request.BuildingId);

            if (slot == null)
            {
                throw new DomainException(
                    errorCode: "SLOT_NOT_FOUND",
                    message: $"Vị trí đỗ xe với ID {request.SlotId.Value} không tồn tại hoặc không thuộc Tòa nhà đã chọn."
                );
            }

            // 3. Kiểm tra xem slot có phù hợp với loại xe không
            if (slot.VehicleTypeId != vehicle.VehicleTypeId)
            {
                throw new DomainException(
                    errorCode: "VEHICLE_TYPE_MISMATCH",
                    message: "Vị trí đỗ xe đã chọn không phù hợp với loại phương tiện của bạn."
                );
            }

            // 4. Kiểm tra xem slot đó có đang bận/bị khóa hay không
            if (slot.Status == SlotStatus.Blocked || slot.Status == SlotStatus.Maintenance)
            {
                throw new DomainException(
                    errorCode: "SLOT_NOT_AVAILABLE",
                    message: "Vị trí đỗ xe đã chọn hiện đang bị khóa hoặc bảo trì."
                );
            }

            // 5. Kiểm tra xem slot đã được đặt bởi booking chồng lấn khác chưa (chỉ tính booking Confirmed hoặc Pending chưa quá hạn thanh toán)
            var isSlotTaken = await _bookingRepository.AnyAsync(b =>
                b.SlotId == request.SlotId.Value &&
                b.BuildingId == request.BuildingId &&
                (b.BookingStatus == BookingStatus.Confirmed || 
                 (b.BookingStatus == BookingStatus.Pending && b.PaymentDeadline > now)) &&
                !(b.PlannedCheckoutTime <= start || b.PlannedCheckinTime >= end));

            if (isSlotTaken)
            {
                throw new DomainException(
                    errorCode: "SLOT_ALREADY_RESERVED",
                    message: "Vị trí đỗ xe đã chọn đã được đặt trước bởi khách hàng khác trong khung giờ này."
                );
            }
        }

        // Bước 6: Tạo entity Booking
        var booking = new BookingEntity
        {
            AccountId = request.AccountId,
            VehicleId = vehicle.Id,
            VehicleTypeId = vehicle.VehicleTypeId,
            BuildingId = request.BuildingId,
            PlannedCheckinTime = plannedCheckinUtc,
            PlannedCheckoutTime = end,
            DepositAmount = depositAmount,
            BookingStatus = BookingStatus.Pending,
            PaymentDeadline = now.AddMinutes(PaymentDeadlineMinutes),
            CheckinGraceUntil = plannedCheckinUtc.AddMinutes(CheckinGracePeriodMinutes),
            SlotId = request.SlotId
        };

        await _bookingRepository.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(booking);
    }

    // -----------------------------------------------------------------------
    // READ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy danh sách toàn bộ Booking.
    /// </summary>
    public async Task<List<BookingDto>> GetAllBookingsAsync(string? status = null)
    {
        var bookings = await _bookingRepository.GetAllWithDetailsAsync();
        if (!string.IsNullOrWhiteSpace(status))
        {
            bookings = bookings.Where(b => string.Equals(b.BookingStatus, status.Trim(), StringComparison.OrdinalIgnoreCase));
        }
        var dtos = new List<BookingDto>();
        foreach (var b in bookings)
        {
            dtos.Add(await MapToDtoAsync(b));
        }
        return dtos;
    }

    /// <summary>
    /// Lấy thông tin Booking theo ID.
    /// </summary>
    public async Task<BookingDto> GetBookingByIdAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);
        if (booking == null)
        {
            throw new DomainException(
                errorCode: "BOOKING_NOT_FOUND",
                message: $"Đặt chỗ với ID {id} không tồn tại."
            );
        }

        return await MapToDtoAsync(booking);
    }

    /// <summary>
    /// Lấy danh sách Booking của một Account.
    /// </summary>
    public async Task<List<BookingDto>> GetBookingsByAccountIdAsync(int accountId, string? status = null)
    {
        var bookings = await _bookingRepository.GetByAccountIdAsync(accountId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            bookings = bookings.Where(b => string.Equals(b.BookingStatus, status.Trim(), StringComparison.OrdinalIgnoreCase));
        }
        var dtos = new List<BookingDto>();
        foreach (var b in bookings)
        {
            dtos.Add(await MapToDtoAsync(b));
        }
        return dtos;
    }

    /// <summary>
    /// Lấy danh sách Booking của một Building.
    /// </summary>
    public async Task<List<BookingDto>> GetBookingsByBuildingIdAsync(int buildingId, string? status = null)
    {
        var bookings = await _bookingRepository.GetByBuildingIdAsync(buildingId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            bookings = bookings.Where(b => string.Equals(b.BookingStatus, status.Trim(), StringComparison.OrdinalIgnoreCase));
        }
        var dtos = new List<BookingDto>();
        foreach (var b in bookings)
        {
            dtos.Add(await MapToDtoAsync(b));
        }
        return dtos;
    }

    // -----------------------------------------------------------------------
    // UPDATE
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật thời gian đặt chỗ — chỉ cho phép khi Booking đang PENDING.
    /// Deposit Fee sẽ được tính lại theo giờ mới.
    /// </summary>
    public async Task<BookingDto> UpdateBookingAsync(int id, UpdateBookingRequest request)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);
        if (booking == null)
        {
            throw new DomainException(
                errorCode: "BOOKING_NOT_FOUND",
                message: $"Đặt chỗ với ID {id} không tồn tại."
            );
        }

        // Chỉ cho phép sửa khi đang PENDING
        if (booking.BookingStatus != BookingStatus.Pending)
        {
            throw new DomainException(
                errorCode: "BOOKING_NOT_EDITABLE",
                message: $"Chỉ có thể cập nhật Booking đang ở trạng thái Pending. " +
                         $"Trạng thái hiện tại: {booking.BookingStatus}."
            );
        }

        var now = DateTime.UtcNow;
        var minAllowed = now.AddMinutes(MinBookingMinutes);

        // Đảm bảo DateTime luôn là UTC cho PostgreSQL
        var plannedCheckinUtc = request.PlannedCheckinTime.Kind == DateTimeKind.Utc
            ? request.PlannedCheckinTime
            : request.PlannedCheckinTime.ToUniversalTime();

        if (plannedCheckinUtc < minAllowed)
        {
            throw new DomainException(
                errorCode: "INVALID_BOOKING_TIME",
                message: $"Thời gian đặt chỗ phải cách hiện tại ít nhất {MinBookingMinutes} phút."
            );
        }

        // Tính lại Deposit Fee theo giờ mới (dùng giờ VN cho pricing lookup)
        var checkinVnOffset = ToVietnamTimeOffset(plannedCheckinUtc);
        var checkinVnDate = checkinVnOffset.Date;
        var pricingPolicy = await _pricingPolicyRepository.GetActivePolicyAsync(
            booking.VehicleTypeId, checkinVnDate);

        if (pricingPolicy != null)
        {
            var checkInTimeOfDay = checkinVnOffset.TimeOfDay;
            var applicableWindow = pricingPolicy.PricingWindows
                .FirstOrDefault(w => IsTimeInWindow(checkInTimeOfDay, w.StartTime, w.EndTime))
                ?? pricingPolicy.PricingWindows.FirstOrDefault();

            if (applicableWindow != null)
            {
                booking.DepositAmount = applicableWindow.BasePrice;
            }
        }

        booking.PlannedCheckinTime = plannedCheckinUtc;
        booking.PlannedCheckoutTime = plannedCheckinUtc.AddHours(2);
        booking.CheckinGraceUntil = plannedCheckinUtc.AddMinutes(CheckinGracePeriodMinutes);

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(booking);
    }

    // -----------------------------------------------------------------------
    // DELETE / CANCEL
    // -----------------------------------------------------------------------

    /// <summary>
    /// Hủy Booking — chỉ khi đang Pending hoặc Confirmed.
    /// </summary>
    public async Task CancelBookingAsync(int id, string? reason = null)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
        {
            throw new DomainException(
                errorCode: "BOOKING_NOT_FOUND",
                message: $"Đặt chỗ với ID {id} không tồn tại."
            );
        }

        // Chỉ hủy được khi Pending hoặc Confirmed
        if (booking.BookingStatus != BookingStatus.Pending &&
            booking.BookingStatus != BookingStatus.Confirmed)
        {
            throw new DomainException(
                errorCode: "BOOKING_NOT_CANCELLABLE",
                message: $"Không thể hủy Booking ở trạng thái '{booking.BookingStatus}'. " +
                         $"Chỉ hủy được khi đang Pending hoặc Confirmed."
            );
        }

        // Kiểm tra chính sách hoàn tiền khi hủy
        if (booking.BookingStatus == BookingStatus.Confirmed)
        {
            var timeRemaining = booking.PlannedCheckinTime - DateTime.UtcNow;

            // Tìm payment đã thanh toán cho booking này
            var payments = await _paymentRepository.FindAsync(p => p.BookingId == booking.Id && p.PaymentStatus == "PAID");
            var payment = payments.FirstOrDefault();

            if (timeRemaining.TotalMinutes >= 60) // Hủy trước 60 phút hoặc sớm hơn -> Hoàn tiền
            {
                if (payment != null)
                {
                    payment.PaymentStatus = "REFUND_PENDING";
                    _paymentRepository.Update(payment);
                }
                booking.CancelReason = $"{(reason ?? "Khách hàng hủy")} (Chờ hoàn cọc)";
            }
            else // Hủy trong vòng 60 phút trước check-in -> Mất cọc
            {
                booking.CancelReason = $"{(reason ?? "Khách hàng hủy muộn")} (Mất cọc)";
            }
        }
        else
        {
            booking.CancelReason = reason ?? "Khách hàng hủy";
        }

        booking.BookingStatus = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // INTERNAL OPERATIONS
    // -----------------------------------------------------------------------

    /// <summary>
    /// Dọn dẹp các Booking hết hạn:
    ///   - PENDING quá hạn thanh toán tiền cọc -> Expired
    ///   - CONFIRMED quá hạn grace period check-in -> NoShow
    /// </summary>
    public async Task CleanupExpiredBookingsAsync()
    {
        var now = DateTime.UtcNow;

        // 1. Pending quá hạn thanh toán cọc -> Expired
        var expiredPendingBookings = await _bookingRepository.FindAsync(b =>
            b.BookingStatus == BookingStatus.Pending &&
            b.PaymentDeadline < now);

        foreach (var booking in expiredPendingBookings)
        {
            booking.BookingStatus = BookingStatus.Expired;
            booking.CancelReason = "Hết hạn thanh toán tiền cọc";
            _bookingRepository.Update(booking);
        }

        // 2. Confirmed quá hạn check-in -> NoShow
        var expiredConfirmedBookings = await _bookingRepository.FindAsync(b =>
            b.BookingStatus == BookingStatus.Confirmed &&
            b.CheckinGraceUntil < now);

        foreach (var booking in expiredConfirmedBookings)
        {
            booking.BookingStatus = BookingStatus.NoShow;
            booking.CancelReason = "Khách hàng không đến (No-Show)";
            _bookingRepository.Update(booking);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // HELPER METHODS
    // -----------------------------------------------------------------------

    /// <summary>
    /// Kiểm tra xem một thời điểm trong ngày có nằm trong khung giờ hay không.
    /// Hỗ trợ khung giờ qua đêm (VD: 22:00 -> 06:00).
    /// </summary>
    private static bool IsTimeInWindow(TimeSpan timeOfDay, TimeSpan start, TimeSpan end)
    {
        if (start < end)
        {
            // Khung giờ thông thường (VD: 06:00 -> 22:00)
            return timeOfDay >= start && timeOfDay < end;
        }
        else
        {
            // Khung giờ qua đêm (VD: 22:00 -> 06:00 hôm sau)
            return timeOfDay >= start || timeOfDay < end;
        }
    }

    /// <summary>
    /// Chuyển đổi Booking entity sang BookingDto để trả về Client.
    /// Tách riêng thành hàm helper để tái sử dụng và tránh lặp code.
    /// </summary>
    private async Task<BookingDto> MapToDtoAsync(BookingEntity booking)
    {
        // Lấy thông tin bổ sung nếu navigation property chưa được load
        string? licensePlate = booking.Vehicle?.LicensePlate;
        string? vehicleTypeName = booking.VehicleType?.TypeName ?? booking.Vehicle?.VehicleType?.TypeName;
        string? buildingName = booking.Building?.Name;
        string? accountName = booking.Account?.FullName;

        // Nếu navigation property chưa được eager load, tra cứu thêm
        if (licensePlate == null)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(booking.VehicleId);
            licensePlate = vehicle?.LicensePlate ?? "N/A";
        }

        if (vehicleTypeName == null)
        {
            var vehicleType = await _vehicleTypeRepository.GetByIdAsync(booking.VehicleTypeId);
            vehicleTypeName = vehicleType?.TypeName;
        }

        if (buildingName == null)
        {
            var building = await _buildingRepository.GetByIdAsync(booking.BuildingId);
            buildingName = building?.Name;
        }

        string? slotCode = booking.ParkingSlot?.Code;
        if (booking.SlotId.HasValue && slotCode == null)
        {
            var slot = await _parkingSlotRepository.GetByIdAsync(booking.SlotId.Value);
            slotCode = slot?.Code;
        }

        return new BookingDto
        {
            Id = booking.Id,
            AccountId = booking.AccountId,
            AccountName = accountName,
            VehicleId = booking.VehicleId,
            LicensePlate = licensePlate,
            VehicleTypeId = booking.VehicleTypeId,
            VehicleTypeName = vehicleTypeName,
            BuildingId = booking.BuildingId,
            BuildingName = buildingName,
            PlannedCheckinTime = booking.PlannedCheckinTime,
            DepositAmount = booking.DepositAmount,
            BookingStatus = booking.BookingStatus,
            PaymentDeadline = booking.PaymentDeadline,
            CheckinGraceUntil = booking.CheckinGraceUntil,
            ConfirmedAt = booking.ConfirmedAt,
            CancelledAt = booking.CancelledAt,
            CancelReason = booking.CancelReason,
            CreatedAt = booking.CreatedAt,
            SlotId = booking.SlotId,
            SlotCode = slotCode
        };
    }
}
