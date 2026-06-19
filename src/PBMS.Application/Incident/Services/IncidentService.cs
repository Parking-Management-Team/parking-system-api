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

    public IncidentService(
        IIncidentRepository incidentRepository,
        IRepository<IncidentType> incidentTypeRepository,
        IRepository<PBMS.Domain.Entities.ParkingSession> sessionRepository,
        IMapper mapper)
    {
        _incidentRepository = incidentRepository;
        _incidentTypeRepository = incidentTypeRepository;
        _sessionRepository = sessionRepository;
        _mapper = mapper;
    }

    public async Task<IncidentDto> ReportIncidentAsync(ReportIncidentRequest request)
    {
        // 1. Kiểm tra session tồn tại
        var session = await _sessionRepository.GetByIdAsync(request.SessionId);
        if (session == null) throw new NotFoundException("PBMS.Domain.Entities.ParkingSession", request.SessionId);

        // 2. Kiểm tra loại sự cố tồn tại
        var incidentType = await _incidentTypeRepository.GetByIdAsync(request.IncidentTypeId);
        if (incidentType == null) throw new NotFoundException("IncidentType", request.IncidentTypeId);

        // 3. Tạo sự cố
        var incident = new PBMS.Domain.Entities.Incident
        {
            SessionId = request.SessionId,
            IncidentTypeId = request.IncidentTypeId,
            Description = request.Description,
            PenaltyFee = request.PenaltyFee ?? incidentType.DefaultPenaltyFee,
            Status = IncidentStatus.Open
        };

        await _incidentRepository.AddAsync(incident);
        await _incidentRepository.SaveChangesAsync();

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
}
