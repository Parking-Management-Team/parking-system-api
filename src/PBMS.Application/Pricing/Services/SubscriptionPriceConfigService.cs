using AutoMapper;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBMS.Application.Pricing.Services;

public class SubscriptionPriceConfigService : ISubscriptionPriceConfigService
{
    private readonly ISubscriptionPriceConfigRepository _repository;
    private readonly IRepository<VehicleType> _vehicleTypeRepository;
    private readonly IMapper _mapper;

    public SubscriptionPriceConfigService(
        ISubscriptionPriceConfigRepository repository,
        IRepository<VehicleType> vehicleTypeRepository,
        IMapper mapper)
    {
        _repository = repository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SubscriptionPriceConfigDto>> GetAllConfigsAsync(int? vehicleTypeId, bool? onlyActive)
    {
        var entities = await _repository.GetAllConfigsWithDetailsAsync(vehicleTypeId, onlyActive);
        return _mapper.Map<IEnumerable<SubscriptionPriceConfigDto>>(entities);
    }

    public async Task<SubscriptionPriceConfigDto?> GetActiveConfigByVehicleTypeAsync(int vehicleTypeId)
    {
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(vehicleTypeId);
        if (vehicleType == null)
        {
            throw new NotFoundException("VehicleType", vehicleTypeId);
        }

        var entity = await _repository.GetActiveConfigByVehicleTypeAsync(vehicleTypeId);
        return entity == null ? null : _mapper.Map<SubscriptionPriceConfigDto>(entity);
    }

    public async Task<SubscriptionPriceConfigDto> CreateConfigAsync(CreateSubscriptionPriceConfigRequest request)
    {
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new NotFoundException("VehicleType", request.VehicleTypeId);
        }

        var now = DateTime.UtcNow;

        // Vô hiệu hóa cấu hình giá đang hoạt động (nếu có)
        var activeConfig = await _repository.GetActiveConfigByVehicleTypeAsync(request.VehicleTypeId);
        if (activeConfig != null)
        {
            activeConfig.IsActive = false;
            activeConfig.EffectiveTo = now;
            _repository.Update(activeConfig);
        }

        // Tạo cấu hình giá mới
        var newConfig = new SubscriptionPriceConfig
        {
            VehicleTypeId = request.VehicleTypeId,
            Price = request.Price,
            EffectiveFrom = now,
            EffectiveTo = null,
            IsActive = true
        };

        await _repository.AddAsync(newConfig);
        await _repository.SaveChangesAsync();

        // Load details for mapper to include VehicleTypeName
        var createdEntity = await _repository.GetByIdAsync(newConfig.Id);
        if (createdEntity != null)
        {
            createdEntity.VehicleType = vehicleType;
        }

        return _mapper.Map<SubscriptionPriceConfigDto>(createdEntity ?? newConfig);
    }

    public async Task<bool> DeactivateConfigAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new NotFoundException("SubscriptionPriceConfig", id);
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
            throw new NotFoundException("SubscriptionPriceConfig", id);
        }

        await _repository.RemoveAsync(entity);
        await _repository.SaveChangesAsync();
        return true;
    }
}
