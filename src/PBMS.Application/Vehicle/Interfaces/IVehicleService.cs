using PBMS.Application.Common;
using PBMS.Application.Vehicle.DTOs;

namespace PBMS.Application.Vehicle.Interfaces;

/// <summary>
/// Service interface for Driver Account & Vehicle Management.
/// </summary>
public interface IVehicleService
{
    Task<BaseResponse<IEnumerable<VehicleDto>>> GetAllAsync();

    Task<BaseResponse<IEnumerable<VehicleDto>>> GetByAccountIdAsync(int accountId);

    Task<BaseResponse<VehicleDto>> GetByIdAsync(int id);

    Task<BaseResponse<VehicleDto>> CreateAsync(CreateVehicleDto createDto);

    Task<BaseResponse<VehicleDto>> UpdateAsync(int id, UpdateVehicleDto updateDto);

    Task<BaseResponse<VehicleDto>> ArchiveAsync(int id);
}
