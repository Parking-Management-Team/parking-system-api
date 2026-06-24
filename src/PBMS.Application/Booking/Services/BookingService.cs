using PBMS.Application.Booking.DTOs;
using PBMS.Application.Booking.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Enums;
using PBMS.Domain.Exceptions;
using BookingEntity = PBMS.Domain.Entities.Booking;
using VehicleEntity = PBMS.Domain.Entities.Vehicle;
using VehicleTypeEntity = PBMS.Domain.Entities.VehicleType;
using BuildingEntity = PBMS.Domain.Entities.Building;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;

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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFeeCalculationService _feeCalculationService;

    // Hằng số nghiệp vụ
    private const int MinBookingHours = 1;
    private const int MaxBookingHours = 8;
    private const int PaymentDeadlineMinutes = 15;
    private const int CheckinGracePeriodMinutes = 30;

    /// <summary>
    /// Constructor nhận các dependency qua Dependency Injection.
    /// </summary>
    public BookingService(
        IBookingRepository bookingRepository,
        IRepository<VehicleEntity> vehicleRepository,
        IRepository<VehicleTypeEntity> vehicleTypeRepository,
        IRepository<BuildingEntity> buildingRepository,
        IBuildingRepository buildingDetailRepository,
        IPricingPolicyRepository pricingPolicyRepository,
        IRepository<ParkingSessionEntity> sessionRepository,
        IUnitOfWork unitOfWork,
        IFeeCalculationService feeCalculationService)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _vehicleTypeRepository = vehicleTypeRepository ?? throw new ArgumentNullException(nameof(vehicleTypeRepository));
        _buildingRepository = buildingRepository ?? throw new ArgumentNullException(nameof(buildingRepository));
        _buildingDetailRepository = buildingDetailRepository ?? throw new ArgumentNullException(nameof(buildingDetailRepository));
        _pricingPolicyRepository = pricingPolicyRepository ?? throw new ArgumentNullException(nameof(pricingPolicyRepository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _feeCalculationService = feeCalculationService ?? throw new ArgumentNullException(nameof(feeCalculationService));
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
        var plannedCheckin = ToUtc(request.PlannedCheckinTime);
        var plannedCheckout = ToUtc(request.PlannedCheckoutTime);
        var now = DateTime.UtcNow;

        // Bước 1: Validate thời gian đặt chỗ
        // Phải từ 1h đến 8h tính từ thời điểm hiện tại
        var minAllowed = now.AddHours(MinBookingHours);
        var maxAllowed = now.AddHours(MaxBookingHours);

        if (plannedCheckin < minAllowed || plannedCheckin > maxAllowed)
        {
            // Display times in Vietnam timezone (UTC+7) for user-friendly error message
            var minVn = minAllowed.AddHours(7);
            var maxVn = maxAllowed.AddHours(7);
            throw new DomainException(
                errorCode: "INVALID_BOOKING_TIME",
                message: $"Thời gian đặt chỗ phải cách hiện tại từ {MinBookingHours} đến {MaxBookingHours} tiếng. " +
                         $"Thời gian hợp lệ: [{minVn:yyyy-MM-dd HH:mm}] đến [{maxVn:yyyy-MM-dd HH:mm}] (Giờ VN)."
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
        // Tổng chỗ General = Tổng capacity Zone General của loại xe
        var totalCapacity = await _buildingDetailRepository.GetTotalGeneralCapacityAsync(
            request.BuildingId, vehicle.VehicleTypeId);

        // Đã sử dụng = Active ParkingSession + Active Booking (Pending | Confirmed)
        var activeSessions = await _sessionRepository.CountAsync(s =>
            s.BuildingId == request.BuildingId &&
            s.Vehicle.VehicleTypeId == vehicle.VehicleTypeId &&
            s.SessionStatus == SessionStatus.Active);

        var activeBookings = await _bookingRepository.GetActiveBookingsCountAsync(
            request.BuildingId, vehicle.VehicleTypeId);

        var usedCapacity = activeSessions + activeBookings;

        if (usedCapacity >= totalCapacity)
        {
            throw new DomainException(
                errorCode: "NO_CAPACITY",
                message: $"Tòa nhà không còn chỗ trống cho loại xe này. " +
                         $"Tổng sức chứa: {totalCapacity}, Đang sử dụng: {usedCapacity}."
            );
        }

        // Bước 5: Tính Deposit Fee (bằng đúng tổng số tiền tạm tính cho toàn bộ thời gian đặt chỗ)
        var feeResult = await _feeCalculationService.CalculateFeeAsync(
            vehicle.VehicleTypeId, plannedCheckin, plannedCheckout);
        decimal depositAmount = feeResult.TotalFee;

        // Bước 6: Tạo entity Booking
        var booking = new BookingEntity
        {
            AccountId = request.AccountId,
            VehicleId = vehicle.Id,
            VehicleTypeId = vehicle.VehicleTypeId,
            BuildingId = request.BuildingId,
            PlannedCheckinTime = plannedCheckin,
            PlannedCheckoutTime = plannedCheckout,
            DepositAmount = depositAmount,
            BookingStatus = BookingStatus.Pending,
            PaymentDeadline = now.AddMinutes(PaymentDeadlineMinutes), // UTC
            CheckinGraceUntil = plannedCheckin.AddMinutes(CheckinGracePeriodMinutes),
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
    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _bookingRepository.GetAllWithDetailsAsync();
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
    public async Task<List<BookingDto>> GetBookingsByAccountIdAsync(int accountId)
    {
        var bookings = await _bookingRepository.GetByAccountIdAsync(accountId);
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
    public async Task<List<BookingDto>> GetBookingsByBuildingIdAsync(int buildingId)
    {
        var bookings = await _bookingRepository.GetByBuildingIdAsync(buildingId);
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
        var plannedCheckin = ToUtc(request.PlannedCheckinTime);
        var plannedCheckout = ToUtc(request.PlannedCheckoutTime);
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
        var minAllowed = now.AddHours(MinBookingHours);
        var maxAllowed = now.AddHours(MaxBookingHours);

        if (plannedCheckin < minAllowed || plannedCheckin > maxAllowed)
        {
            throw new DomainException(
                errorCode: "INVALID_BOOKING_TIME",
                message: $"Thời gian đặt chỗ phải cách hiện tại từ {MinBookingHours} đến {MaxBookingHours} tiếng."
            );
        }

        // Tính lại Deposit Fee theo giờ mới
        var feeResult = await _feeCalculationService.CalculateFeeAsync(
            booking.VehicleTypeId, plannedCheckin, plannedCheckout);
        booking.DepositAmount = feeResult.TotalFee;

        booking.PlannedCheckinTime = plannedCheckin;
        booking.PlannedCheckoutTime = plannedCheckout;
        booking.CheckinGraceUntil = plannedCheckin.AddMinutes(CheckinGracePeriodMinutes);

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

        booking.BookingStatus = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancelReason = reason ?? "Khách hàng hủy";

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
    /// Hỗ trợ khung giờ qua đêm (VD: 22:00 → 06:00).
    /// </summary>
    private static bool IsTimeInWindow(TimeSpan timeOfDay, TimeSpan start, TimeSpan end)
    {
        if (start < end)
        {
            // Khung giờ thông thường (VD: 06:00 → 22:00)
            return timeOfDay >= start && timeOfDay < end;
        }
        else
        {
            // Khung giờ qua đêm (VD: 22:00 → 06:00 hôm sau)
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
        };
    }

    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
