using AutoMapper;
using PBMS.Application.Blacklist.DTOs;
using PBMS.Application.Blacklist.Interfaces;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;

namespace PBMS.Application.Blacklist.Services;

public class BlacklistService : IBlacklistService
{
    private readonly IBlacklistRepository _blacklistRepository;
    private readonly IRepository<PBMS.Domain.Entities.Vehicle> _vehicleRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IRepository<PBMS.Domain.Entities.Incident> _incidentRepository;
    private readonly IMapper _mapper;

    public BlacklistService(
        IBlacklistRepository blacklistRepository,
        IRepository<PBMS.Domain.Entities.Vehicle> vehicleRepository,
        ICardRepository cardRepository,
        IRepository<PBMS.Domain.Entities.Incident> incidentRepository,
        IMapper mapper)
    {
        _blacklistRepository = blacklistRepository;
        _vehicleRepository = vehicleRepository;
        _cardRepository = cardRepository;
        _incidentRepository = incidentRepository;
        _mapper = mapper;
    }

    public async Task<BlacklistDto> AddToBlacklistAsync(AddToBlacklistRequest request)
    {
        int? vehicleId = request.VehicleId;
        int? cardId = request.CardId;

        // 1. Tìm Vehicle theo LicensePlate nếu không có VehicleId
        if (!vehicleId.HasValue && !string.IsNullOrWhiteSpace(request.LicensePlate))
        {
            var vehicle = await _vehicleRepository.FirstOrDefaultAsync(
                v => v.LicensePlate.ToUpper() == request.LicensePlate.Trim().ToUpper());
            if (vehicle == null)
                throw new NotFoundException("Vehicle", $"LicensePlate '{request.LicensePlate}'");
            vehicleId = vehicle.Id;
        }

        // 2. Tìm Card theo CardCode nếu không có CardId
        if (!cardId.HasValue && !string.IsNullOrWhiteSpace(request.CardCode))
        {
            var card = await _cardRepository.GetByCardCodeAsync(request.CardCode.Trim());
            if (card == null)
                throw new NotFoundException("Card", $"CardCode '{request.CardCode}'");
            cardId = card.Id;
        }

        // 3. Validate至少 có 1 đối tượng bị chặn
        if (!vehicleId.HasValue && !cardId.HasValue && !request.IncidentId.HasValue)
        {
            throw new ValidationException("You must provide at least a Vehicle, Card, or Incident to blacklist.");
        }

        // 4. Kiểm tra tồn tại nếu dùng ID trực tiếp
        if (vehicleId.HasValue && request.VehicleId.HasValue)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId.Value);
            if (vehicle == null) throw new NotFoundException("Vehicle", vehicleId.Value);
        }

        if (cardId.HasValue && request.CardId.HasValue)
        {
            var card = await _cardRepository.GetByIdAsync(cardId.Value);
            if (card == null) throw new NotFoundException("Card", cardId.Value);
        }

        if (request.IncidentId.HasValue)
        {
            var incident = await _incidentRepository.GetByIdAsync(request.IncidentId.Value);
            if (incident == null) throw new NotFoundException("Incident", request.IncidentId.Value);
        }

        // 5. Tạo bản ghi Blacklist
        var blacklist = new PBMS.Domain.Entities.Blacklist
        {
            VehicleId = vehicleId,
            CardId = cardId,
            IncidentId = request.IncidentId,
            Reason = request.Reason
        };

        await _blacklistRepository.AddAsync(blacklist);
        await _blacklistRepository.SaveChangesAsync();

        return _mapper.Map<BlacklistDto>(blacklist);
    }

    public async Task RemoveFromBlacklistAsync(int id)
    {
        var blacklist = await _blacklistRepository.GetByIdAsync(id);
        if (blacklist == null) throw new NotFoundException("Blacklist", id);

        await _blacklistRepository.RemoveAsync(blacklist);
        await _blacklistRepository.SaveChangesAsync();
    }

    public async Task<BlacklistDto> GetBlacklistByIdAsync(int id)
    {
        var blacklist = await _blacklistRepository.GetBlacklistWithDetailsAsync(id);

        if (blacklist == null) throw new NotFoundException("Blacklist", id);

        return _mapper.Map<BlacklistDto>(blacklist);
    }

    public async Task<PagedResult<BlacklistDto>> GetBlacklistPagedAsync(int pageIndex, int pageSize)
    {
        var pagedResult = await _blacklistRepository.GetPagedBlacklistWithDetailsAsync(pageIndex, pageSize);

        var dtos = _mapper.Map<IEnumerable<BlacklistDto>>(pagedResult.Items);

        return new PagedResult<BlacklistDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages,
            PageIndex = pagedResult.PageIndex,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<bool> IsVehicleBlockedAsync(int vehicleId)
    {
        return await _blacklistRepository.AnyAsync(b => b.VehicleId == vehicleId);
    }

    public async Task<bool> IsCardBlockedAsync(int cardId)
    {
        return await _blacklistRepository.AnyAsync(b => b.CardId == cardId);
    }
}
