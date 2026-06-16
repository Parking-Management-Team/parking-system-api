using AutoMapper;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.Services;

/// <summary>
/// Triển khai service cho logic nghiệp vụ ParkingSlot.
/// </summary>
public class ParkingSlotService : IParkingSlotService
{
    private readonly IParkingSlotRepository _slotRepository;
    private readonly IRepository<Zone> _zoneRepository;
    private readonly IRepository<VehicleType> _vehicleTypeRepository;
    private readonly IMapper _mapper;

    public ParkingSlotService(
        IParkingSlotRepository slotRepository,
        IRepository<Zone> zoneRepository,
        IRepository<VehicleType> vehicleTypeRepository,
        IMapper mapper)
    {
        _slotRepository = slotRepository;
        _zoneRepository = zoneRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _mapper = mapper;
    }

    public async Task<ParkingSlotDto> CreateSlotAsync(ParkingSlotCreateRequest request)
    {
        // 1. Kiểm tra Zone tồn tại
        var zone = await _zoneRepository.GetByIdAsync(request.ZoneId);
        if (zone == null)
        {
            throw new NotFoundException("Zone", request.ZoneId);
        }

        // 2. Kiểm tra VehicleType tồn tại
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new NotFoundException("VehicleType", request.VehicleTypeId);
        }

        // 3. Kiểm tra SlotCode duy nhất
        var codeExists = await _slotRepository.SlotCodeExistsAsync(request.Code);
        if (codeExists)
        {
            throw new ValidationException($"Slot code '{request.Code}' already exists.");
        }

        // 4. Tạo entity
        var slot = new ParkingSlot
        {
            ZoneId = request.ZoneId,
            VehicleTypeId = request.VehicleTypeId,
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name,
            Status = SlotStatus.Available
        };

        await _slotRepository.AddAsync(slot);
        return _mapper.Map<ParkingSlotDto>(slot);
    }

    public async Task<ParkingSlotDto> GetSlotByIdAsync(int id)
    {
        var slot = await _slotRepository.GetSlotWithDetailsAsync(id);
        if (slot == null)
        {
            throw new NotFoundException("ParkingSlot", id);
        }

        return _mapper.Map<ParkingSlotDto>(slot);
    }

    public async Task<IEnumerable<ParkingSlotDto>> GetAllSlotsAsync()
    {
        var slots = await _slotRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ParkingSlotDto>>(slots);
    }

    public async Task<IEnumerable<ParkingSlotDto>> GetSlotsByZoneAsync(int zoneId)
    {
        var zone = await _zoneRepository.GetByIdAsync(zoneId);
        if (zone == null)
        {
            throw new NotFoundException("Zone", zoneId);
        }

        var slots = await _slotRepository.GetSlotsByZoneIdAsync(zoneId);
        return _mapper.Map<IEnumerable<ParkingSlotDto>>(slots);
    }

    public async Task<PagedResult<ParkingSlotDto>> GetSlotsPagedAsync(int pageIndex, int pageSize)
    {
        var slots = await _slotRepository.GetAllAsync();
        var totalCount = slots.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var pagedSlots = slots
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var slotDtos = _mapper.Map<IEnumerable<ParkingSlotDto>>(pagedSlots);

        return new PagedResult<ParkingSlotDto>
        {
            Items = slotDtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<ParkingSlotDto> UpdateSlotAsync(int id, ParkingSlotUpdateRequest request)
    {
        var slot = await _slotRepository.GetByIdAsync(id);
        if (slot == null)
        {
            throw new NotFoundException("ParkingSlot", id);
        }

        // Kiểm tra VehicleType tồn tại
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new NotFoundException("VehicleType", request.VehicleTypeId);
        }

        // Kiểm tra SlotCode mới nếu thay đổi
        var newCode = request.Code.Trim().ToUpper();
        if (slot.Code != newCode)
        {
            var codeExists = await _slotRepository.SlotCodeExistsAsync(newCode);
            if (codeExists)
            {
                throw new ValidationException($"Slot code '{newCode}' already exists.");
            }
        }

        slot.Code = newCode;
        slot.Name = request.Name;
        slot.VehicleTypeId = request.VehicleTypeId;
        slot.Status = request.Status;

        _slotRepository.Update(slot);
        return _mapper.Map<ParkingSlotDto>(slot);
    }

    public async Task DeleteSlotAsync(int id)
    {
        var slot = await _slotRepository.GetSlotWithDetailsAsync(id);
        if (slot == null)
        {
            throw new NotFoundException("ParkingSlot", id);
        }

        // Logic bảo vệ: Không cho xóa nếu đang có xe đỗ hoặc đã đặt chỗ
        if (slot.Status == SlotStatus.Occupied)
        {
            throw new ValidationException($"Cannot delete slot '{slot.Code}' because its status is {slot.Status}.");
        }

        // Kiểm tra ParkingSessions
        if (slot.ParkingSessions.Any(ps => string.Equals(ps.SessionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException($"Cannot delete slot '{slot.Code}' because it has an active parking session.");
        }

        await _slotRepository.RemoveAsync(slot);
    }
}
