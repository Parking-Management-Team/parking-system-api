using PBMS.Application.Incident.DTOs;

namespace PBMS.Application.Incident.Interfaces;

public interface IIncidentTypeService
{
    Task<IEnumerable<IncidentTypeDto>> GetAllIncidentTypesAsync();
    Task<IncidentTypeDto> GetIncidentTypeByIdAsync(int id);
    Task<IncidentTypeDto> CreateIncidentTypeAsync(CreateIncidentTypeRequest request);
    Task<IncidentTypeDto> UpdateIncidentTypeAsync(int id, UpdateIncidentTypeRequest request);
    Task DeleteIncidentTypeAsync(int id);
}
