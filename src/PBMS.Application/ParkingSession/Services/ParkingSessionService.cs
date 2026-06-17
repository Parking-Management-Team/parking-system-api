using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingSession.Services;

public class ParkingSessionService : IParkingSessionService
{
    private const string ActiveSessionStatus = "Active";

    private readonly IParkingSessionRepository _parkingSessionRepository;
    private readonly IRepository<Vehicle> _vehicleRepository;
    private readonly IRepository<VehicleType> _vehicleTypeRepository;
    private readonly ICardRepository _cardRepository;

    public ParkingSessionService(
        IParkingSessionRepository parkingSessionRepository,
        IRepository<Vehicle> vehicleRepository,
        IRepository<VehicleType> vehicleTypeRepository,
        ICardRepository cardRepository)
    {
        _parkingSessionRepository = parkingSessionRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _cardRepository = cardRepository;
    }

    public async Task<ParkingSessionDto> CheckInAsync(CheckInRequest request)
    {
        var normalizedPlate = Normalize(request.LicensePlate);
        var normalizedCardCode = Normalize(request.CardCode);

        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new NotFoundException("VehicleType", request.VehicleTypeId);
        }

        var card = await _cardRepository.GetByCardCodeAsync(normalizedCardCode);
        if (card == null)
        {
            throw new NotFoundException($"Card with code '{normalizedCardCode}' was not found.");
        }

        if (card.CardStatus != CardStatus.Available.ToString())
        {
            throw new ValidationException("Card is not available for check-in.");
        }

        var vehicle = await _parkingSessionRepository.GetVehicleByLicensePlateAsync(normalizedPlate);
        if (vehicle != null && vehicle.VehicleTypeId != request.VehicleTypeId)
        {
            throw new ValidationException("License plate already exists with a different vehicle type.");
        }

        if (vehicle != null && await _parkingSessionRepository.HasActiveSessionForVehicleAsync(vehicle.Id))
        {
            throw new ValidationException("Vehicle is already inside the parking lot.");
        }

        vehicle ??= new Vehicle
        {
            LicensePlate = normalizedPlate,
            VehicleTypeId = request.VehicleTypeId
        };

        if (vehicle.Id == 0)
        {
            await _vehicleRepository.AddAsync(vehicle);
        }

        Zone? assignedZone;
        ParkingSlot? assignedSlot = null;

        if (IsCar(vehicleType))
        {
            assignedSlot = await _parkingSessionRepository.FindAvailableGeneralSlotAsync(
                request.VehicleTypeId,
                request.BuildingId);

            if (assignedSlot == null)
            {
                throw new ValidationException("No available GENERAL slot found for this vehicle type.");
            }

            assignedZone = assignedSlot.Zone;
            assignedSlot.Status = SlotStatus.Occupied;
        }
        else
        {
            assignedZone = await _parkingSessionRepository.FindAvailableZoneAsync(
                request.VehicleTypeId,
                request.BuildingId);

            if (assignedZone == null)
            {
                throw new ValidationException("No available zone found for this vehicle type.");
            }
        }

        var session = new Domain.Entities.ParkingSession
        {
            Vehicle = vehicle,
            VehicleId = vehicle.Id,
            CardId = card.Id,
            ZoneId = assignedZone.Id,
            Zone = assignedZone,
            ParkingSlotId = assignedSlot?.Id,
            ParkingSlot = assignedSlot,
            CheckInTime = DateTime.UtcNow,
            InStaffId = request.StaffId,
            SessionStatus = ActiveSessionStatus
        };

        card.CardStatus = CardStatus.Active.ToString();

        await _parkingSessionRepository.AddAsync(session);
        _cardRepository.Update(card);

        await _parkingSessionRepository.SaveChangesAsync();

        return MapToDto(session, vehicle, card, assignedZone, assignedSlot);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static bool IsCar(VehicleType vehicleType)
    {
        var code = vehicleType.Code?.Trim().ToUpperInvariant() ?? string.Empty;
        var name = vehicleType.Name?.Trim().ToUpperInvariant() ?? string.Empty;

        return code == "CAR" ||
               code == "AUTOMOBILE" ||
               name.Contains("CAR") ||
               name.Contains("AUTOMOBILE");
    }

    private static ParkingSessionDto MapToDto(
        Domain.Entities.ParkingSession session,
        Vehicle vehicle,
        Domain.Entities.Card card,
        Zone zone,
        ParkingSlot? slot)
    {
        return new ParkingSessionDto
        {
            Id = session.Id,
            VehicleId = vehicle.Id,
            LicensePlate = vehicle.LicensePlate,
            VehicleTypeId = vehicle.VehicleTypeId,
            CardId = card.Id,
            CardCode = card.CardCode,
            ZoneId = zone.Id,
            ZoneCode = zone.Code,
            ParkingSlotId = slot?.Id,
            SlotCode = slot?.Code,
            CheckInTime = session.CheckInTime,
            SessionStatus = session.SessionStatus
        };
    }
}
