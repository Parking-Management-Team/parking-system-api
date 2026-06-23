using AutoMapper;
using PBMS.Application.Contracts;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.Incident.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Application.Common.Exceptions;
using System.Linq;

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

    public async Task<IncidentTypeDto> CreateIncidentTypeAsync(CreateIncidentTypeRequest request)
    {
        var existing = await _incidentTypeRepository.FindAsync(it => it.IncidentCode == request.IncidentCode);
        if (existing.Any())
        {
            throw new ValidationException($"IncidentType with code '{request.IncidentCode}' already exists.");
        }

        var incidentType = new IncidentType
        {
            IncidentCode = request.IncidentCode,
            IncidentName = request.IncidentName,
            Description = request.Description
        };

        await _incidentTypeRepository.AddAsync(incidentType);
        await _incidentTypeRepository.SaveChangesAsync();

        return _mapper.Map<IncidentTypeDto>(incidentType);
    }

    public async Task<IncidentTypeDto> UpdateIncidentTypeAsync(int id, UpdateIncidentTypeRequest request)
    {
        var incidentType = await _incidentTypeRepository.GetByIdAsync(id);
        if (incidentType == null) throw new NotFoundException("IncidentType", id);

        incidentType.IncidentName = request.IncidentName;
        incidentType.Description = request.Description;

        _incidentTypeRepository.Update(incidentType);
        await _incidentTypeRepository.SaveChangesAsync();

        return _mapper.Map<IncidentTypeDto>(incidentType);
    }

    public async Task DeleteIncidentTypeAsync(int id)
    {
        var incidentType = await _incidentTypeRepository.GetByIdAsync(id);
        if (incidentType == null) throw new NotFoundException("IncidentType", id);

        await _incidentTypeRepository.RemoveAsync(incidentType);
        await _incidentTypeRepository.SaveChangesAsync();
    }
}
