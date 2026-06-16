using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Application.MonthlyCard.DTOs;
using PBMS.Application.MonthlyCard.Interfaces;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBMS.Application.MonthlyCard.Services;

public class MonthlySubscriptionService : IMonthlySubscriptionService
{
    private readonly IMonthlySubscriptionRepository _monthlySubRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBuildingRepository _buildingRepository;

    public MonthlySubscriptionService(
        IMonthlySubscriptionRepository monthlySubRepository,
        ICardRepository cardRepository,
        IVehicleRepository vehicleRepository,
        IAccountRepository accountRepository,
        IBuildingRepository buildingRepository)
    {
        _monthlySubRepository = monthlySubRepository;
        _cardRepository = cardRepository;
        _vehicleRepository = vehicleRepository;
        _accountRepository = accountRepository;
        _buildingRepository = buildingRepository;
    }

    public async Task<MonthlySubscriptionDto> RegisterSubscriptionAsync(CreateMonthlySubscriptionRequest request)
    {
        // 1. Kiểm tra tài khoản tồn tại
        var account = await _accountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
        {
            throw new DomainException("ACCOUNT_NOT_FOUND", $"Tài khoản ID {request.AccountId} không tồn tại.");
        }

        // 2. Kiểm tra phương tiện tồn tại
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new DomainException("VEHICLE_NOT_FOUND", $"Phương tiện ID {request.VehicleId} không tồn tại.");
        }

        // 3. Kiểm tra tòa nhà tồn tại
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId);
        if (building == null)
        {
            throw new DomainException("BUILDING_NOT_FOUND", $"Tòa nhà ID {request.BuildingId} không tồn tại.");
        }

        // 4. Kiểm tra phương tiện đã có vé tháng ACTIVE chưa
        var existingActiveSub = await _monthlySubRepository.GetActiveSubscriptionByVehicleIdAsync(request.VehicleId);
        if (existingActiveSub != null)
        {
            throw new DomainException("ACTIVE_SUBSCRIPTION_EXISTS", $"Phương tiện này đang có vé tháng hoạt động (ID: {existingActiveSub.Id}).");
        }

        // 5. Tạo hồ sơ vé tháng PENDING
        var subscription = new MonthlySubscription
        {
            AccountId = request.AccountId,
            VehicleId = request.VehicleId,
            BuildingId = request.BuildingId,
            MonthlyPrice = request.MonthlyPrice,
            MonthlySubscriptionStatus = "PENDING",
            CreatedAt = DateTime.UtcNow
        };

        await _monthlySubRepository.AddAsync(subscription);
        await _monthlySubRepository.SaveChangesAsync();

        return MapToDto(subscription);
    }

    public async Task<MonthlySubscriptionDto> ActivateSubscriptionAsync(int subscriptionId, string cardCode)
    {
        var subscription = await _monthlySubRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Không tìm thấy vé tháng ID {subscriptionId}.");
        }

        if (subscription.MonthlySubscriptionStatus == "ACTIVE")
        {
            throw new DomainException("SUBSCRIPTION_ALREADY_ACTIVE", "Vé tháng này đã được kích hoạt.");
        }

        // 1. Tìm thẻ bằng CardCode
        var card = await _cardRepository.GetByCardCodeAsync(cardCode);
        if (card == null)
        {
            throw new DomainException("CARD_NOT_FOUND", $"Không tìm thấy thẻ có mã '{cardCode}' trong hệ thống.");
        }

        // 2. Kiểm tra thẻ có khả dụng không
        if (!string.Equals(card.CardStatus, PBMS.Domain.Enums.CardStatus.Available.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("CARD_NOT_AVAILABLE", $"Thẻ '{cardCode}' không ở trạng thái sẵn sàng (Available). Trạng thái hiện tại: {card.CardStatus}.");
        }

        // 3. Kiểm tra thẻ đã được gán cho vé tháng ACTIVE nào khác chưa
        var isAssigned = await _monthlySubRepository.IsCardAssignedToActiveSubscriptionAsync(card.Id);
        if (isAssigned)
        {
            throw new DomainException("CARD_ALREADY_ASSIGNED", $"Thẻ '{cardCode}' đã được gán cho một vé tháng hoạt động khác.");
        }

        // 4. Kích hoạt và gán thẻ
        subscription.AssignedCardId = card.Id;
        subscription.ActivatedAt = DateTime.UtcNow;
        subscription.ExpiredAt = DateTime.UtcNow.AddMonths(1);
        subscription.MonthlySubscriptionStatus = "ACTIVE";

        _monthlySubRepository.Update(subscription);
        await _monthlySubRepository.SaveChangesAsync();

        // Nạp lại các navigation properties để lấy thông tin trả về
        var updatedSub = await _monthlySubRepository.GetActiveSubscriptionByCardCodeAsync(cardCode);
        return MapToDto(updatedSub ?? subscription);
    }

    public async Task<MonthlySubscriptionDto> RenewSubscriptionAsync(int subscriptionId, int months)
    {
        var subscription = await _monthlySubRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Không tìm thấy vé tháng ID {subscriptionId}.");
        }

        var now = DateTime.UtcNow;
        if (subscription.ExpiredAt.HasValue && subscription.ExpiredAt.Value > now)
        {
            subscription.ExpiredAt = subscription.ExpiredAt.Value.AddMonths(months);
        }
        else
        {
            subscription.ActivatedAt = now;
            subscription.ExpiredAt = now.AddMonths(months);
        }

        subscription.MonthlySubscriptionStatus = "ACTIVE";
        _monthlySubRepository.Update(subscription);
        await _monthlySubRepository.SaveChangesAsync();

        return MapToDto(subscription);
    }

    public async Task<MonthlySubscriptionDto> ReportLostCardAsync(int subscriptionId)
    {
        var subscription = await _monthlySubRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Không tìm thấy vé tháng ID {subscriptionId}.");
        }

        if (subscription.AssignedCardId.HasValue)
        {
            var card = await _cardRepository.GetByIdAsync(subscription.AssignedCardId.Value);
            if (card != null)
            {
                // Báo mất thẻ
                card.CardStatus = PBMS.Domain.Enums.CardStatus.Lost.ToString();
                _cardRepository.Update(card);
            }

            // Gỡ liên kết thẻ khỏi vé tháng
            subscription.AssignedCardId = null;
            _monthlySubRepository.Update(subscription);
            await _monthlySubRepository.SaveChangesAsync();
        }

        return MapToDto(subscription);
    }

    public async Task<MonthlySubscriptionDto> ReassignCardAsync(int subscriptionId, string newCardCode)
    {
        var subscription = await _monthlySubRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Không tìm thấy vé tháng ID {subscriptionId}.");
        }

        // 1. Tìm thẻ mới
        var newCard = await _cardRepository.GetByCardCodeAsync(newCardCode);
        if (newCard == null)
        {
            throw new DomainException("CARD_NOT_FOUND", $"Không tìm thấy thẻ có mã '{newCardCode}'.");
        }

        // 2. Kiểm tra thẻ mới khả dụng
        if (!string.Equals(newCard.CardStatus, PBMS.Domain.Enums.CardStatus.Available.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("CARD_NOT_AVAILABLE", $"Thẻ '{newCardCode}' không ở trạng thái sẵn sàng (Available).");
        }

        // 3. Kiểm tra thẻ mới đã được gán cho vé tháng nào khác chưa
        var isAssigned = await _monthlySubRepository.IsCardAssignedToActiveSubscriptionAsync(newCard.Id);
        if (isAssigned)
        {
            throw new DomainException("CARD_ALREADY_ASSIGNED", $"Thẻ '{newCardCode}' đã được gán cho một vé tháng hoạt động khác.");
        }

        // 4. Nếu vé tháng cũ đang có thẻ, chúng ta giải phóng thẻ cũ (chuyển về Available nếu nó không bị Lost/Blocked)
        if (subscription.AssignedCardId.HasValue)
        {
            var oldCard = await _cardRepository.GetByIdAsync(subscription.AssignedCardId.Value);
            if (oldCard != null && string.Equals(oldCard.CardStatus, PBMS.Domain.Enums.CardStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                oldCard.CardStatus = PBMS.Domain.Enums.CardStatus.Available.ToString();
                _cardRepository.Update(oldCard);
            }
        }

        // 5. Gán thẻ mới
        subscription.AssignedCardId = newCard.Id;
        _monthlySubRepository.Update(subscription);
        await _monthlySubRepository.SaveChangesAsync();

        var updatedSub = await _monthlySubRepository.GetActiveSubscriptionByCardCodeAsync(newCardCode);
        return MapToDto(updatedSub ?? subscription);
    }

    public async Task<MonthlySubscriptionDto> GetByIdAsync(int id)
    {
        var subscription = await _monthlySubRepository.GetByIdAsync(id);
        if (subscription == null)
        {
            throw new DomainException("SUBSCRIPTION_NOT_FOUND", $"Không tìm thấy vé tháng ID {id}.");
        }

        return MapToDto(subscription);
    }

    public async Task<IEnumerable<MonthlySubscriptionDto>> GetAllAsync()
    {
        var subs = await _monthlySubRepository.GetAllAsync();
        return subs.Select(MapToDto).ToList();
    }

    private static MonthlySubscriptionDto MapToDto(MonthlySubscription sub)
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
}
