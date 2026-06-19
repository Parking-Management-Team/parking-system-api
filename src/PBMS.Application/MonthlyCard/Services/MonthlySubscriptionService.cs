using PBMS.Application.Contracts;
using PBMS.Application.MonthlyCard.DTOs;
using PBMS.Application.MonthlyCard.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleEntity = PBMS.Domain.Entities.Vehicle;
using CardEntity = PBMS.Domain.Entities.Card;


namespace PBMS.Application.MonthlyCard.Services;

/// <summary>
/// Triển khai dịch vụ nghiệp vụ đăng ký và quản lý vé tháng (IMonthlySubscriptionService).
/// </summary>
public class MonthlySubscriptionService : IMonthlySubscriptionService
{
    private readonly IMonthlySubscriptionRepository _subscriptionRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly IRepository<VehicleType> _vehicleTypeRepository;
    private readonly IParkingSlotRepository _parkingSlotRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MonthlySubscriptionService(
        IMonthlySubscriptionRepository subscriptionRepository,
        ICardRepository cardRepository,
        IBuildingRepository buildingRepository,
        IRepository<VehicleEntity> vehicleRepository,
        IRepository<VehicleType> vehicleTypeRepository,
        IParkingSlotRepository parkingSlotRepository,
        IRepository<Account> accountRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _cardRepository = cardRepository;
        _buildingRepository = buildingRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _parkingSlotRepository = parkingSlotRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }


    /// <summary>
    /// Đăng ký vé tháng mới (Trạng thái mặc định: PENDING).
    /// </summary>
    public async Task<MonthlySubscriptionDto> RegisterSubscriptionAsync(CreateSubscriptionRequest request)
    {
        // 1. Kiểm tra Account tồn tại
        var account = await _accountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
        {
            throw new DomainException("ACCOUNT_NOT_FOUND", $"Tài khoản với ID {request.AccountId} không tồn tại.");
        }

        // 2. Kiểm tra Xe tồn tại
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new DomainException("VEHICLE_NOT_FOUND", $"Phương tiện với ID {request.VehicleId} không tồn tại.");
        }

        // 3. Kiểm tra Tòa nhà tồn tại
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId);
        if (building == null)
        {
            throw new DomainException("BUILDING_NOT_FOUND", $"Tòa nhà với ID {request.BuildingId} không tồn tại.");
        }

        // 4. Lấy loại xe
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(vehicle.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new DomainException("VEHICLE_TYPE_NOT_FOUND", $"Loại xe với ID {vehicle.VehicleTypeId} không tồn tại.");
        }

        // 5. Kiểm tra đăng ký chồng lấn (chỉ cho phép 1 ACTIVE hoặc PENDING cho cùng 1 xe)
        var hasOverlap = await _subscriptionRepository.HasOverlapSubscriptionAsync(request.VehicleId);
        if (hasOverlap)
        {
            throw new DomainException("OVERLAP_SUBSCRIPTION", $"Xe có biển số '{vehicle.LicensePlate}' đã có đăng ký tháng đang hoạt động hoặc đang chờ thanh toán.");
        }

        // 6. Kiểm tra thẻ đỗ xe nếu được truyền vào
        CardEntity? assignedCard = null;
        if (request.AssignedCardId.HasValue)
        {
            assignedCard = await _cardRepository.GetByIdAsync(request.AssignedCardId.Value);
            if (assignedCard == null)
            {
                throw new DomainException("CARD_NOT_FOUND", $"Thẻ gửi xe với ID {request.AssignedCardId.Value} không tồn tại.");
            }

            if (assignedCard.CardType.ToUpper() != "MONTHLY")
            {
                throw new DomainException("INVALID_CARD_TYPE", $"Thẻ '{assignedCard.CardCode}' không phải là loại thẻ MONTHLY.");
            }

            if (assignedCard.CardStatus != CardStatus.Available.ToString())
            {
                throw new DomainException("CARD_NOT_AVAILABLE", $"Thẻ '{assignedCard.CardCode}' không ở trạng thái Available.");
            }
        }

        int? assignedSlotId = null;
        decimal monthlyPrice = 0;

        // 7. Xử lý logic đỗ xe theo loại xe
        if (vehicleType.TypeName == VehicleType.MotorcycleTypeName)
        {
            // Xe máy -> Kiểm tra sức chứa động
            var activeAndPendingCount = await _subscriptionRepository.GetActiveAndPendingMotorcycleSubscriptionsCountAsync(request.BuildingId);
            var totalMotorcycleCapacity = await _buildingRepository.GetTotalMotorcycleCapacityAsync(request.BuildingId);

            if (activeAndPendingCount >= totalMotorcycleCapacity)
            {
                throw new DomainException("CAPACITY_FULL", "Tòa nhà đã hết sức chứa trống cho đăng ký xe máy tháng.");
            }

            monthlyPrice = 120000m; // Giá vé tháng mặc định cho xe máy
        }
        else if (vehicleType.TypeName == VehicleType.CarTypeName)
        {
            // Ô tô -> Tìm và giữ một slot đỗ trong Zone MONTHLY
            var availableSlot = await _parkingSlotRepository.FindAvailableMonthlySlotAsync(request.BuildingId, vehicle.VehicleTypeId);
            if (availableSlot == null)
            {
                throw new DomainException("SLOT_NOT_AVAILABLE", "Tòa nhà không còn chỗ đỗ trống trong khu vực xe tháng (MONTHLY) cho ô tô.");
            }

            assignedSlotId = availableSlot.Id;
            monthlyPrice = 1500000m; // Giá vé tháng mặc định cho ô tô
        }
        else
        {
            throw new DomainException("UNSUPPORTED_VEHICLE_TYPE", $"Không hỗ trợ đăng ký vé tháng cho loại xe '{vehicleType.TypeName}'.");
        }

        // 8. Tạo mới MonthlySubscription
        var subscription = new MonthlySubscription
        {
            AccountId = request.AccountId,
            VehicleId = request.VehicleId,
            BuildingId = request.BuildingId,
            AssignedCardId = request.AssignedCardId,
            AssignedSlotId = assignedSlotId,
            MonthlyPrice = monthlyPrice,
            MonthlySubscriptionStatus = MonthlySubscriptionStatus.Pending
        };

        await _subscriptionRepository.AddAsync(subscription);
        await _unitOfWork.SaveChangesAsync();

        return await MapAsync(subscription);
    }

    /// <summary>
    /// Lấy thông tin đăng ký vé tháng theo ID.
    /// </summary>
    public async Task<MonthlySubscriptionDto> GetSubscriptionByIdAsync(int id)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Đăng ký vé tháng với ID {id} không tồn tại.");
        }

        return await MapAsync(subscription);
    }

    /// <summary>
    /// Hủy đăng ký vé tháng.
    /// </summary>
    public async Task CancelSubscriptionAsync(int id)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Đăng ký vé tháng với ID {id} không tồn tại.");
        }

        subscription.MonthlySubscriptionStatus = MonthlySubscriptionStatus.Cancelled;

        // Trả trạng thái thẻ liên kết về Available nếu có
        if (subscription.AssignedCardId.HasValue)
        {
            var card = await _cardRepository.GetByIdAsync(subscription.AssignedCardId.Value);
            if (card != null && card.CardStatus == CardStatus.Assigned.ToString())
            {
                card.CardStatus = CardStatus.Available.ToString();
                _cardRepository.Update(card);
            }
        }

        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Dọn dẹp các hồ sơ PENDING quá hạn thanh toán.
    /// </summary>
    public async Task CleanupExpiredPendingSubscriptionsAsync(int timeoutMinutes)
    {
        var pendingSubscriptions = await _subscriptionRepository.GetTimeoutPendingSubscriptionsAsync(timeoutMinutes);
        foreach (var sub in pendingSubscriptions)
        {
            sub.MonthlySubscriptionStatus = MonthlySubscriptionStatus.Cancelled;

            if (sub.AssignedCardId.HasValue)
            {
                var card = await _cardRepository.GetByIdAsync(sub.AssignedCardId.Value);
                if (card != null && card.CardStatus == CardStatus.Assigned.ToString())
                {
                    card.CardStatus = CardStatus.Available.ToString();
                    _cardRepository.Update(card);
                }
            }

            _subscriptionRepository.Update(sub);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Chuyển đổi thực thể MonthlySubscription sang DTO.
    /// </summary>
    private async Task<MonthlySubscriptionDto> MapAsync(MonthlySubscription sub)
    {
        string? cardCode = null;
        if (sub.AssignedCardId.HasValue)
        {
            var card = await _cardRepository.GetByIdAsync(sub.AssignedCardId.Value);
            cardCode = card?.CardCode;
        }

        string? slotCode = null;
        if (sub.AssignedSlotId.HasValue)
        {
            var slot = await _parkingSlotRepository.GetByIdAsync(sub.AssignedSlotId.Value);
            slotCode = slot?.Code;
        }

        return new MonthlySubscriptionDto
        {
            Id = sub.Id,
            AccountId = sub.AccountId,
            VehicleId = sub.VehicleId,
            AssignedCardId = sub.AssignedCardId,
            CardCode = cardCode,
            AssignedSlotId = sub.AssignedSlotId,
            SlotCode = slotCode,
            BuildingId = sub.BuildingId,
            MonthlyPrice = sub.MonthlyPrice,
            ActivatedAt = sub.ActivatedAt,
            ExpiredAt = sub.ExpiredAt,
            MonthlySubscriptionStatus = sub.MonthlySubscriptionStatus,
            CreatedAt = sub.CreatedAt
        };
    }
}
