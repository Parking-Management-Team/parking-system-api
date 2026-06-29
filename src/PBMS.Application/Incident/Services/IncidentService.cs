using AutoMapper;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.Incident.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Application.Incident.Services;

public class IncidentService : IIncidentService
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IRepository<IncidentType> _incidentTypeRepository;
    private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepository;
    private readonly IMapper _mapper;
    private readonly IFeeCalculatorService _feeCalculatorService;
    private readonly IPenaltyConfigRepository _penaltyConfigRepository;
    private readonly ICardRepository _cardRepository;

    public IncidentService(
        IIncidentRepository incidentRepository,
        IRepository<IncidentType> incidentTypeRepository,
        IRepository<PBMS.Domain.Entities.ParkingSession> sessionRepository,
        IMapper mapper,
        IFeeCalculatorService feeCalculatorService,
        IPenaltyConfigRepository penaltyConfigRepository,
        ICardRepository cardRepository)
    {
        _incidentRepository = incidentRepository;
        _incidentTypeRepository = incidentTypeRepository;
        _sessionRepository = sessionRepository;
        _mapper = mapper;
        _feeCalculatorService = feeCalculatorService;
        _penaltyConfigRepository = penaltyConfigRepository;
        _cardRepository = cardRepository;
    }

    public async Task<IncidentDto> ReportIncidentAsync(ReportIncidentRequest request)
    {
        // Kiểm tra tiền phạt không âm
        if (request.PenaltyFee.HasValue && request.PenaltyFee.Value < 0)
        {
            throw new ValidationException("Penalty fee cannot be negative.");
        }

        // 1. Kiểm tra session tồn tại
        var session = await _sessionRepository.GetByIdAsync(request.SessionId);
        if (session == null) throw new NotFoundException("PBMS.Domain.Entities.ParkingSession", request.SessionId);

        // Ràng buộc trạng thái session
        if (session.SessionStatus.ToUpper() == "COMPLETED")
        {
            throw new ValidationException("Cannot report an incident on a completed parking session.");
        }

        // 2. Kiểm tra loại sự cố tồn tại
        var incidentType = await _incidentTypeRepository.GetByIdAsync(request.IncidentTypeId);
        if (incidentType == null) throw new NotFoundException("IncidentType", request.IncidentTypeId);

        // Ngăn chặn báo cáo sự cố trùng loại (LOST_CARD, LATE_CHECKOUT)
        if (incidentType.IncidentCode != null && (incidentType.IncidentCode.ToUpper() == "LOST_CARD" || incidentType.IncidentCode.ToUpper() == "LATE_CHECKOUT"))
        {
            var existingIncident = await _incidentRepository.FirstOrDefaultAsync(i => 
                i.SessionId == request.SessionId && 
                i.IncidentTypeId == request.IncidentTypeId && 
                i.Status == IncidentStatus.Open);
            if (existingIncident != null)
            {
                throw new ValidationException($"An open incident of type '{incidentType.IncidentName}' already exists for this session.");
            }
        }

        // 3. Lấy cấu hình giá phạt
        var activePenaltyConfig = await _penaltyConfigRepository.GetActiveConfigByIncidentTypeAsync(request.IncidentTypeId);
        var calculatedFee = activePenaltyConfig?.PenaltyFee ?? 0;

        // 4. Tạo sự cố
        var incident = new PBMS.Domain.Entities.Incident
        {
            SessionId = request.SessionId,
            IncidentTypeId = request.IncidentTypeId,
            Description = request.Description,
            PenaltyFee = request.PenaltyFee ?? calculatedFee,
            PenaltyConfigId = activePenaltyConfig?.Id,
            Status = IncidentStatus.Open
        };

        await _incidentRepository.AddAsync(incident);
        await _incidentRepository.SaveChangesAsync();

        // Tự động cập nhật trạng thái thẻ khi báo cáo mất thẻ
        if (incidentType.IncidentCode != null && incidentType.IncidentCode.ToUpper() == "LOST_CARD")
        {
            var card = await _cardRepository.GetByIdAsync(session.CardId);
            if (card != null)
            {
                card.CardStatus = CardStatus.Lost.ToString();
                _cardRepository.Update(card);
                await _cardRepository.SaveChangesAsync();
            }
        }

        return _mapper.Map<IncidentDto>(incident);
    }

    public async Task<IncidentDto> UpdateIncidentStatusAsync(int id, UpdateIncidentStatusRequest request)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null) throw new NotFoundException("Incident", id);

        incident.Status = request.Status;
        if (!string.IsNullOrEmpty(request.Description))
        {
            incident.Description = request.Description;
        }

        if (request.Status == IncidentStatus.Resolved)
        {
            incident.ResolvedAt = DateTime.UtcNow;

            // Nếu là sự cố LOST_CARD, khóa thẻ đỗ xe vĩnh viễn (Blocked)
            var incidentType = await _incidentTypeRepository.GetByIdAsync(incident.IncidentTypeId);
            if (incidentType != null && incidentType.IncidentCode != null && incidentType.IncidentCode.ToUpper() == "LOST_CARD")
            {
                var session = await _sessionRepository.GetByIdAsync(incident.SessionId);
                if (session != null)
                {
                    var card = await _cardRepository.GetByIdAsync(session.CardId);
                    if (card != null && card.CardStatus == CardStatus.Lost.ToString())
                    {
                        card.CardStatus = CardStatus.Blocked.ToString();
                        _cardRepository.Update(card);
                        await _cardRepository.SaveChangesAsync();
                    }
                }
            }
        }

        _incidentRepository.Update(incident);
        await _incidentRepository.SaveChangesAsync();

        return _mapper.Map<IncidentDto>(incident);
    }

    public async Task<IncidentDto> GetIncidentByIdAsync(int id)
    {
        var incident = await _incidentRepository.GetIncidentWithDetailsAsync(id);

        if (incident == null) throw new NotFoundException("Incident", id);

        return _mapper.Map<IncidentDto>(incident);
    }

    public async Task<PagedResult<IncidentDto>> GetIncidentsPagedAsync(int pageIndex, int pageSize)
    {
        var pagedResult = await _incidentRepository.GetPagedIncidentsWithDetailsAsync(pageIndex, pageSize);

        var dtos = _mapper.Map<IEnumerable<IncidentDto>>(pagedResult.Items);

        return new PagedResult<IncidentDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages,
            PageIndex = pagedResult.PageIndex,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<IEnumerable<IncidentDto>> GetIncidentsBySessionAsync(int sessionId)
    {
        var items = await _incidentRepository.GetIncidentsBySessionWithDetailsAsync(sessionId);
        return _mapper.Map<IEnumerable<IncidentDto>>(items);
    }

    public async Task<IncidentDto> UpdateIncidentAsync(int id, UpdateIncidentRequest request)
    {
        if (request.PenaltyFee.HasValue && request.PenaltyFee.Value < 0)
        {
            throw new ValidationException("Penalty fee cannot be negative.");
        }

        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null) throw new NotFoundException("Incident", id);

        if (request.IncidentTypeId.HasValue && request.IncidentTypeId.Value != incident.IncidentTypeId)
        {
            var incidentType = await _incidentTypeRepository.GetByIdAsync(request.IncidentTypeId.Value);
            if (incidentType == null) throw new NotFoundException("IncidentType", request.IncidentTypeId.Value);
            incident.IncidentTypeId = request.IncidentTypeId.Value;
        }

        if (request.Description != null)
        {
            incident.Description = request.Description;
        }

        if (request.PenaltyFee.HasValue)
        {
            incident.PenaltyFee = request.PenaltyFee.Value;
        }

        _incidentRepository.Update(incident);
        await _incidentRepository.SaveChangesAsync();

        return _mapper.Map<IncidentDto>(incident);
    }

    public async Task DeleteIncidentAsync(int id)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null) throw new NotFoundException("Incident", id);

        await _incidentRepository.RemoveAsync(incident);
        await _incidentRepository.SaveChangesAsync();
    }
}
