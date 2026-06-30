using PBMS.Application.AuditLog.Interfaces;
using PBMS.Application.Common;
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
/// Implementation of monthly subscription service (IMonthlySubscriptionService).
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
    private readonly ISubscriptionPriceConfigRepository _priceConfigRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;

    public MonthlySubscriptionService(
        IMonthlySubscriptionRepository subscriptionRepository,
        ICardRepository cardRepository,
        IBuildingRepository buildingRepository,
        IRepository<VehicleEntity> vehicleRepository,
        IRepository<VehicleType> vehicleTypeRepository,
        IParkingSlotRepository parkingSlotRepository,
        IRepository<Account> accountRepository,
        ISubscriptionPriceConfigRepository priceConfigRepository,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _cardRepository = cardRepository;
        _buildingRepository = buildingRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _parkingSlotRepository = parkingSlotRepository;
        _accountRepository = accountRepository;
        _priceConfigRepository = priceConfigRepository;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
    }


    /// <summary>
    /// Register a new monthly subscription (default status: PENDING).
    /// </summary>
    public async Task<MonthlySubscriptionDto> RegisterSubscriptionAsync(CreateSubscriptionRequest request)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
        {
            throw new DomainException("ACCOUNT_NOT_FOUND", $"Account with ID {request.AccountId} not found.");
        }

        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new DomainException("VEHICLE_NOT_FOUND", $"Vehicle with ID {request.VehicleId} not found.");
        }

        var building = await _buildingRepository.GetByIdAsync(request.BuildingId);
        if (building == null)
        {
            throw new DomainException("BUILDING_NOT_FOUND", $"Building with ID {request.BuildingId} not found.");
        }

        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(vehicle.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new DomainException("VEHICLE_TYPE_NOT_FOUND", $"Vehicle type with ID {vehicle.VehicleTypeId} not found.");
        }

        var hasOverlap = await _subscriptionRepository.HasOverlapSubscriptionAsync(request.VehicleId);
        if (hasOverlap)
        {
            throw new DomainException("OVERLAP_SUBSCRIPTION", $"Vehicle '{vehicle.LicensePlate}' already has an active or pending subscription.");
        }

        CardEntity? assignedCard = null;
        if (request.AssignedCardId.HasValue)
        {
            assignedCard = await _cardRepository.GetByIdAsync(request.AssignedCardId.Value);
            if (assignedCard == null)
            {
                throw new DomainException("CARD_NOT_FOUND", $"Card with ID {request.AssignedCardId.Value} not found.");
            }

            if (assignedCard.CardType.ToUpper() != "MONTHLY")
            {
                throw new DomainException("INVALID_CARD_TYPE", $"Card '{assignedCard.CardCode}' is not a MONTHLY card.");
            }

            if (assignedCard.CardStatus != CardStatus.Available.ToString())
            {
                throw new DomainException("CARD_NOT_AVAILABLE", $"Card '{assignedCard.CardCode}' is not available.");
            }
        }

        int? assignedSlotId = null;
        decimal monthlyPrice = 0;

        // Get price from SubscriptionPriceConfig
        var priceConfig = await _priceConfigRepository.GetActiveConfigByVehicleTypeAsync(vehicleType.Id);
        if (priceConfig == null)
        {
            throw new DomainException("NO_PRICE_CONFIG", $"No active subscription price configuration found for vehicle type '{vehicleType.TypeName}'.");
        }
        monthlyPrice = priceConfig.Price;

        if (vehicleType.TypeName == VehicleType.MotorcycleTypeName)
        {
            var activeAndPendingCount = await _subscriptionRepository.GetActiveAndPendingMotorcycleSubscriptionsCountAsync(request.BuildingId);
            var totalMotorcycleCapacity = await _buildingRepository.GetTotalMotorcycleCapacityAsync(request.BuildingId);

            if (activeAndPendingCount >= totalMotorcycleCapacity)
            {
                throw new DomainException("CAPACITY_FULL", "Building has no available capacity for monthly motorcycle subscriptions.");
            }
        }
        else if (vehicleType.TypeName == VehicleType.CarTypeName)
        {
            var availableSlot = await _parkingSlotRepository.FindAvailableMonthlySlotAsync(request.BuildingId, vehicle.VehicleTypeId);
            if (availableSlot == null)
            {
                throw new DomainException("SLOT_NOT_AVAILABLE", "Building has no available monthly parking slots for cars.");
            }

            assignedSlotId = availableSlot.Id;
        }
        else
        {
            throw new DomainException("UNSUPPORTED_VEHICLE_TYPE", $"Monthly subscription is not supported for vehicle type '{vehicleType.TypeName}'.");
        }

        var subscription = new MonthlySubscription
        {
            AccountId = request.AccountId,
            VehicleId = request.VehicleId,
            BuildingId = request.BuildingId,
            AssignedCardId = request.AssignedCardId,
            AssignedSlotId = assignedSlotId,
            MonthlyPrice = monthlyPrice,
            SubscriptionPriceConfigId = priceConfig.Id,
            MonthlySubscriptionStatus = MonthlySubscriptionStatus.Pending
        };

        await _subscriptionRepository.AddAsync(subscription);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            request.AccountId,
            "CREATE",
            "monthly_subscription",
            subscription.Id,
            $"Registered monthly subscription for vehicle '{vehicle.LicensePlate}' with price {monthlyPrice:N0} VND");

        return await MapAsync(subscription);
    }

    /// <summary>
    /// Get monthly subscription by ID.
    /// </summary>
    public async Task<MonthlySubscriptionDto> GetSubscriptionByIdAsync(int id)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Monthly subscription with ID {id} not found.");
        }

        return await MapAsync(subscription);
    }

    /// <summary>
    /// Cancel monthly subscription.
    /// </summary>
    public async Task CancelSubscriptionAsync(int id)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Monthly subscription with ID {id} not found.");
        }

        // Block cancel for expired subscriptions
        if (subscription.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Expired)
        {
            throw new DomainException("CANNOT_CANCEL_EXPIRED", "Cannot cancel an expired subscription.");
        }

        // Block cancel for already cancelled subscriptions
        if (subscription.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Cancelled)
        {
            throw new DomainException("ALREADY_CANCELLED", "Subscription is already cancelled.");
        }

        var previousStatus = subscription.MonthlySubscriptionStatus;
        subscription.MonthlySubscriptionStatus = MonthlySubscriptionStatus.Cancelled;

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

        await _auditLogService.LogAsync(
            null,
            "CANCEL",
            "monthly_subscription",
            subscription.Id,
            $"Cancelled subscription ID {id} from status {previousStatus}");
    }

    /// <summary>
    /// Update a monthly subscription (card only).
    /// </summary>
    public async Task<MonthlySubscriptionDto> UpdateSubscriptionAsync(int id, UpdateSubscriptionRequest request)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Monthly subscription with ID {id} not found.");
        }

        if (subscription.MonthlySubscriptionStatus != MonthlySubscriptionStatus.Pending &&
            subscription.MonthlySubscriptionStatus != MonthlySubscriptionStatus.Active)
        {
            throw new DomainException("INVALID_STATUS", $"Cannot update subscription with status {subscription.MonthlySubscriptionStatus}.");
        }

        if (!request.AssignedCardId.HasValue || request.AssignedCardId == subscription.AssignedCardId)
        {
            return await MapAsync(subscription);
        }

        var newCard = await _cardRepository.GetByIdAsync(request.AssignedCardId.Value);
        if (newCard == null)
        {
            throw new DomainException("CARD_NOT_FOUND", $"Card with ID {request.AssignedCardId} not found.");
        }

        if (newCard.CardType.ToUpper() != "MONTHLY")
        {
            throw new DomainException("INVALID_CARD_TYPE", $"Card '{newCard.CardCode}' is not a MONTHLY card.");
        }

        if (newCard.CardStatus != CardStatus.Available.ToString())
        {
            throw new DomainException("CARD_NOT_AVAILABLE", $"Card '{newCard.CardCode}' is not available.");
        }

        var oldCardId = subscription.AssignedCardId;

        if (subscription.AssignedCardId.HasValue)
        {
            var oldCard = await _cardRepository.GetByIdAsync(subscription.AssignedCardId.Value);
            if (oldCard != null && oldCard.CardStatus == CardStatus.Assigned.ToString())
            {
                oldCard.CardStatus = CardStatus.Available.ToString();
                _cardRepository.Update(oldCard);
            }
        }

        subscription.AssignedCardId = request.AssignedCardId.Value;

        if (subscription.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Active)
        {
            newCard.CardStatus = CardStatus.Assigned.ToString();
            _cardRepository.Update(newCard);
        }

        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            null,
            "UPDATE",
            "monthly_subscription",
            subscription.Id,
            $"Updated card from {oldCardId} to {request.AssignedCardId} for subscription ID {id}");

        return await MapAsync(subscription);
    }

    /// <summary>
    /// Cleanup expired pending subscriptions.
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
    /// Get list of monthly subscriptions with filtering and pagination.
    /// </summary>
    public async Task<PagedResult<MonthlySubscriptionDto>> GetAllSubscriptionsAsync(MonthlySubscriptionFilterRequest filter)
    {
        var (items, totalCount) = await _subscriptionRepository.GetPagedAsync(
            filter.Page,
            filter.PageSize,
            filter.Status,
            filter.BuildingId,
            filter.AccountId,
            filter.LicensePlate,
            filter.CardCode);

        var dtos = new List<MonthlySubscriptionDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapFromEntityAsync(item));
        }

        return PagedResult<MonthlySubscriptionDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    /// <summary>
    /// Activate monthly subscription (PENDING -> ACTIVE).
    /// </summary>
    public async Task<MonthlySubscriptionDto> ActivateSubscriptionAsync(int id)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Monthly subscription with ID {id} not found.");
        }

        if (subscription.MonthlySubscriptionStatus != MonthlySubscriptionStatus.Pending)
        {
            throw new DomainException("INVALID_STATUS", $"Only PENDING subscriptions can be activated. Current status: {subscription.MonthlySubscriptionStatus}.");
        }

        subscription.MonthlySubscriptionStatus = MonthlySubscriptionStatus.Active;
        subscription.ActivatedAt = DateTime.UtcNow;

        int durationDays = 30;
        if (subscription.SubscriptionPriceConfigId.HasValue)
        {
            var priceConfig = await _priceConfigRepository.GetByIdAsync(subscription.SubscriptionPriceConfigId.Value);
            if (priceConfig != null)
            {
                durationDays = priceConfig.DurationDays;
            }
        }
        subscription.ExpiredAt = subscription.ActivatedAt.Value.AddDays(durationDays);


        if (subscription.AssignedCardId.HasValue)
        {
            var card = await _cardRepository.GetByIdAsync(subscription.AssignedCardId.Value);
            if (card != null && card.CardStatus == CardStatus.Available.ToString())
            {
                card.CardStatus = CardStatus.Assigned.ToString();
                _cardRepository.Update(card);
            }
        }

        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            null,
            "ACTIVATE",
            "monthly_subscription",
            subscription.Id,
            $"Activated subscription ID {id}, expires at {subscription.ExpiredAt:yyyy-MM-dd}");

        return await MapAsync(subscription);
    }

    private async Task<MonthlySubscriptionDto> MapFromEntityAsync(MonthlySubscription sub)
    {
        return new MonthlySubscriptionDto
        {
            Id = sub.Id,
            AccountId = sub.AccountId,
            VehicleId = sub.VehicleId,
            AssignedCardId = sub.AssignedCardId,
            CardCode = sub.AssignedCard?.CardCode,
            AssignedSlotId = sub.AssignedSlotId,
            SlotCode = sub.AssignedSlot?.Code,
            BuildingId = sub.BuildingId,
            MonthlyPrice = sub.MonthlyPrice,
            ActivatedAt = sub.ActivatedAt,
            ExpiredAt = sub.ExpiredAt,
            MonthlySubscriptionStatus = sub.MonthlySubscriptionStatus,
            CreatedAt = sub.CreatedAt
        };
    }

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

    public async Task<MonthlySubscriptionDto> ReplaceSubscriptionCardAsync(int subscriptionId, string newCardCode)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Monthly subscription with ID {subscriptionId} not found.");
        }

        if (subscription.MonthlySubscriptionStatus != "Active")
        {
            throw new DomainException("SUBSCRIPTION_NOT_ACTIVE", $"Only ACTIVE subscriptions can replace card. Current status: {subscription.MonthlySubscriptionStatus}.");
        }

        var normalizedCardCode = newCardCode.Trim().ToUpper();
        var newCard = await _cardRepository.GetByCardCodeAsync(normalizedCardCode);
        if (newCard == null)
        {
            throw new DomainException("CARD_NOT_FOUND", $"Card with code '{newCardCode}' not found.");
        }

        if (newCard.CardStatus != CardStatus.Available.ToString())
        {
            throw new DomainException("CARD_NOT_AVAILABLE", $"Card '{newCard.CardCode}' is not available (Status: {newCard.CardStatus}).");
        }

        // 1. Cập nhật thẻ cũ sang trạng thái LOST và đặt LostAt
        if (subscription.AssignedCardId.HasValue)
        {
            var oldCard = await _cardRepository.GetByIdAsync(subscription.AssignedCardId.Value);
            if (oldCard != null)
            {
                oldCard.CardStatus = CardStatus.Lost.ToString();
                oldCard.LostAt = DateTime.UtcNow.AddHours(7);
                _cardRepository.Update(oldCard);
            }
        }

        // 2. Cập nhật thẻ mới sang trạng thái ACTIVE
        newCard.CardStatus = CardStatus.Active.ToString();
        _cardRepository.Update(newCard);

        // 3. Cập nhật subscription liên kết với thẻ mới
        subscription.AssignedCardId = newCard.Id;
        _subscriptionRepository.Update(subscription);

        await _subscriptionRepository.SaveChangesAsync();

        return await MapAsync(subscription);
    }

    public async Task<MonthlySubscriptionDto> RenewSubscriptionAsync(int id)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Monthly subscription with ID {id} not found.");
        }

        if (subscription.MonthlySubscriptionStatus != "Active" && subscription.MonthlySubscriptionStatus != "Expired")
        {
            throw new DomainException("INVALID_STATUS", $"Only ACTIVE or EXPIRED subscriptions can be renewed. Current status: {subscription.MonthlySubscriptionStatus}.");
        }

        // Get the vehicle and vehicle type to check current pricing config
        var vehicle = await _vehicleRepository.GetByIdAsync(subscription.VehicleId);
        if (vehicle == null)
        {
            throw new DomainException("VEHICLE_NOT_FOUND", $"Vehicle with ID {subscription.VehicleId} not found.");
        }

        var priceConfig = await _priceConfigRepository.GetActiveConfigByVehicleTypeAsync(vehicle.VehicleTypeId);
        if (priceConfig == null)
        {
            throw new DomainException("NO_PRICE_CONFIG", "No active subscription price configuration found for renewal.");
        }

        // Extend ExpirationDate: 
        // If current subscription is ACTIVE and not yet expired, new expiration = ExpiredAt + DurationDays.
        // If it is EXPIRED, new expiration = DateTime.UtcNow + DurationDays.
        var baseDate = (subscription.MonthlySubscriptionStatus == "Active" && subscription.ExpiredAt.HasValue && subscription.ExpiredAt.Value > DateTime.UtcNow) 
            ? subscription.ExpiredAt.Value 
            : DateTime.UtcNow;

        subscription.ExpiredAt = baseDate.AddDays(priceConfig.DurationDays);
        subscription.MonthlyPrice = priceConfig.Price;
        subscription.MonthlySubscriptionStatus = "Active"; // Ensure it is active


        _subscriptionRepository.Update(subscription);
        await _subscriptionRepository.SaveChangesAsync();

        return await MapAsync(subscription);
    }
}