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
        // 1. Kiểm tra tồn tại của các liên kết nếu được cung cấp
        if (request.VehicleId.HasValue)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId.Value);
            if (vehicle == null) throw new NotFoundException("Vehicle", request.VehicleId.Value);
        }

        if (request.CardId.HasValue)
        {
            var card = await _cardRepository.GetByIdAsync(request.CardId.Value);
            if (card == null) throw new NotFoundException("Card", request.CardId.Value);
        }

        if (request.IncidentId.HasValue)
        {
            var incident = await _incidentRepository.GetByIdAsync(request.IncidentId.Value);
            if (incident == null) throw new NotFoundException("Incident", request.IncidentId.Value);
        }

        // 2. Tạo bản ghi Blacklist
        var blacklist = new PBMS.Domain.Entities.Blacklist
        {
            VehicleId = request.VehicleId,
            CardId = request.CardId,
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
