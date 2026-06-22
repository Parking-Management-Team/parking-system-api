using AutoMapper;
using PBMS.Application.Contracts;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.Incident.Interfaces;
using PBMS.Domain.Entities;

namespace PBMS.Application.Incident.Services;

public class IncidentTypeService : IIncidentTypeService
{
    private readonly IRepository<IncidentType> _incidentTypeRepository;
    private readonly IMapper _mapper;

    public IncidentTypeService(IRepository<IncidentType> incidentTypeRepository, IMapper mapper)
    {
        _incidentTypeRepository = incidentTypeRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<IncidentTypeDto>> GetAllIncidentTypesAsync()
    {
        var items = await _incidentTypeRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<IncidentTypeDto>>(items);
    }

    public async Task<IncidentTypeDto> GetIncidentTypeByIdAsync(int id)
    {
        var item = await _incidentTypeRepository.GetByIdAsync(id);
        return _mapper.Map<IncidentTypeDto>(item);
    }
}
