using AutoMapper;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.Incident.Interfaces;
using PBMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBMS.Application.Incident.Services;

public class PenaltyConfigService : IPenaltyConfigService
{
    private readonly IPenaltyConfigRepository _repository;
    private readonly IRepository<IncidentType> _incidentTypeRepository;
    private readonly IMapper _mapper;

    public PenaltyConfigService(
        IPenaltyConfigRepository repository,
        IRepository<IncidentType> incidentTypeRepository,
        IMapper mapper)
    {
        _repository = repository;
        _incidentTypeRepository = incidentTypeRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PenaltyConfigDto>> GetAllConfigsAsync(int? incidentTypeId, bool? onlyActive)
    {
        var entities = await _repository.GetAllConfigsWithDetailsAsync(incidentTypeId, onlyActive);
        return _mapper.Map<IEnumerable<PenaltyConfigDto>>(entities);
    }

    public async Task<PenaltyConfigDto?> GetActiveConfigByIncidentTypeAsync(int incidentTypeId)
    {
        var incidentType = await _incidentTypeRepository.GetByIdAsync(incidentTypeId);
        if (incidentType == null)
        {
            throw new NotFoundException("IncidentType", incidentTypeId);
        }

        var entity = await _repository.GetActiveConfigByIncidentTypeAsync(incidentTypeId);
        return entity == null ? null : _mapper.Map<PenaltyConfigDto>(entity);
    }

    public async Task<PenaltyConfigDto> CreateConfigAsync(CreatePenaltyConfigRequest request)
    {
        var incidentType = await _incidentTypeRepository.GetByIdAsync(request.IncidentTypeId);
        if (incidentType == null)
        {
            throw new NotFoundException("IncidentType", request.IncidentTypeId);
        }

        var now = DateTime.UtcNow;

        // Vô hiệu hóa cấu hình giá phạt đang hoạt động (nếu có)
        var activeConfig = await _repository.GetActiveConfigByIncidentTypeAsync(request.IncidentTypeId);
        if (activeConfig != null)
        {
            activeConfig.IsActive = false;
            activeConfig.EffectiveTo = now;
            _repository.Update(activeConfig);
        }

        // Tạo cấu hình giá phạt mới
        var newConfig = new PenaltyConfig
        {
            IncidentTypeId = request.IncidentTypeId,
            PenaltyFee = request.PenaltyFee,
            EffectiveFrom = now,
            EffectiveTo = null,
            IsActive = true
        };

        await _repository.AddAsync(newConfig);
        await _repository.SaveChangesAsync();

        // Load details for mapper to include IncidentTypeName
        var createdEntity = await _repository.GetByIdAsync(newConfig.Id);
        if (createdEntity != null)
        {
            createdEntity.IncidentType = incidentType;
        }

        return _mapper.Map<PenaltyConfigDto>(createdEntity ?? newConfig);
    }

    public async Task<bool> DeactivateConfigAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new NotFoundException("PenaltyConfig", id);
        }

        if (entity.IsActive)
        {
            entity.IsActive = false;
            entity.EffectiveTo = DateTime.UtcNow;
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteConfigAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new NotFoundException("PenaltyConfig", id);
        }

        await _repository.RemoveAsync(entity);
        await _repository.SaveChangesAsync();
        return true;
    }
}
