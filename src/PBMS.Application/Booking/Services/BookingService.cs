using PBMS.Application.Booking.DTOs;
using PBMS.Application.Booking.Interfaces;
using PBMS.Application.Contracts;
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
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _vehicleTypeRepository = vehicleTypeRepository ?? throw new ArgumentNullException(nameof(vehicleTypeRepository));
        _buildingRepository = buildingRepository ?? throw new ArgumentNullException(nameof(buildingRepository));
        _buildingDetailRepository = buildingDetailRepository ?? throw new ArgumentNullException(nameof(buildingDetailRepository));
        _pricingPolicyRepository = pricingPolicyRepository ?? throw new ArgumentNullException(nameof(pricingPolicyRepository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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

        // Bước 1: Validate thời gian đặt chỗ
        // Phải từ 1h đến 8h tính từ thời điểm hiện tại
        var minAllowed = now.AddHours(MinBookingHours);
        var maxAllowed = now.AddHours(MaxBookingHours);

        if (request.PlannedCheckinTime < minAllowed || request.PlannedCheckinTime > maxAllowed)
        {
            throw new DomainException(
                errorCode: "INVALID_BOOKING_TIME",
                message: $"Thời gian đặt chỗ phải cách hiện tại từ {MinBookingHours} đến {MaxBookingHours} tiếng. " +
                         $"Thời gian hợp lệ: [{minAllowed:yyyy-MM-dd HH:mm} UTC] đến [{maxAllowed:yyyy-MM-dd HH:mm} UTC]."
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

        // Bước 5: Tính Deposit Fee
        // Tra cứu PricingPolicy Active cho loại xe tại thời điểm check-in dự kiến
        var pricingPolicy = await _pricingPolicyRepository.GetActivePolicyAsync(
            vehicle.VehicleTypeId, request.PlannedCheckinTime);

        if (pricingPolicy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: "Không tìm thấy chính sách giá phù hợp cho loại xe này tại thời điểm đặt chỗ."
            );
        }

        // Tìm PricingWindow tương ứng với giờ check-in dự kiến
        var checkInTimeOfDay = request.PlannedCheckinTime.TimeOfDay;
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

        // Bước 6: Tạo entity Booking
        var booking = new BookingEntity
        {
            AccountId = request.AccountId,
            VehicleId = vehicle.Id,
            VehicleTypeId = vehicle.VehicleTypeId,
            BuildingId = request.BuildingId,
            PlannedCheckinTime = request.PlannedCheckinTime,
            PlannedCheckoutTime = request.PlannedCheckinTime.AddHours(2), // Mặc định dự kiến 2h
            DepositAmount = depositAmount,
            BookingStatus = BookingStatus.Pending,
            PaymentDeadline = now.AddMinutes(PaymentDeadlineMinutes),
            CheckinGraceUntil = request.PlannedCheckinTime.AddMinutes(CheckinGracePeriodMinutes),
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

        if (request.PlannedCheckinTime < minAllowed || request.PlannedCheckinTime > maxAllowed)
        {
            throw new DomainException(
                errorCode: "INVALID_BOOKING_TIME",
                message: $"Thời gian đặt chỗ phải cách hiện tại từ {MinBookingHours} đến {MaxBookingHours} tiếng."
            );
        }

        // Tính lại Deposit Fee theo giờ mới
        var pricingPolicy = await _pricingPolicyRepository.GetActivePolicyAsync(
            booking.VehicleTypeId, request.PlannedCheckinTime);

        if (pricingPolicy != null)
        {
            var checkInTimeOfDay = request.PlannedCheckinTime.TimeOfDay;
            var applicableWindow = pricingPolicy.PricingWindows
                .FirstOrDefault(w => IsTimeInWindow(checkInTimeOfDay, w.StartTime, w.EndTime))
                ?? pricingPolicy.PricingWindows.FirstOrDefault();

            if (applicableWindow != null)
            {
                booking.DepositAmount = applicableWindow.BasePrice;
            }
        }

        booking.PlannedCheckinTime = request.PlannedCheckinTime;
        booking.PlannedCheckoutTime = request.PlannedCheckinTime.AddHours(2);
        booking.CheckinGraceUntil = request.PlannedCheckinTime.AddMinutes(CheckinGracePeriodMinutes);

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
    /// Dọn dẹp các Booking PENDING đã quá PaymentDeadline → chuyển sang Expired.
    /// </summary>
    public async Task CleanupExpiredPendingBookingsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredBookings = await _bookingRepository.FindAsync(b =>
            b.BookingStatus == BookingStatus.Pending &&
            b.PaymentDeadline < now);

        foreach (var booking in expiredBookings)
        {
            booking.BookingStatus = BookingStatus.Expired;
            booking.CancelReason = "Hết hạn thanh toán tiền cọc";
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
}
