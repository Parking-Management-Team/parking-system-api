using PBMS.Application.Incident.DTOs;

namespace PBMS.Application.Incident.Interfaces;

public interface IIncidentTypeService
{
    Task<IEnumerable<IncidentTypeDto>> GetAllIncidentTypesAsync();
    Task<IncidentTypeDto> GetIncidentTypeByIdAsync(int id);
}
