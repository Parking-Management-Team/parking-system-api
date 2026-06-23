using PBMS.Application.Common;
using PBMS.Application.Incident.DTOs;

namespace PBMS.Application.Incident.Interfaces;

public interface IIncidentService
{
    Task<IncidentDto> ReportIncidentAsync(ReportIncidentRequest request);
    Task<IncidentDto> UpdateIncidentStatusAsync(int id, UpdateIncidentStatusRequest request);
    Task<IncidentDto> GetIncidentByIdAsync(int id);
    Task<PagedResult<IncidentDto>> GetIncidentsPagedAsync(int pageIndex, int pageSize);
    Task<IEnumerable<IncidentDto>> GetIncidentsBySessionAsync(int sessionId);
    Task<IncidentDto> UpdateIncidentAsync(int id, UpdateIncidentRequest request);
    Task DeleteIncidentAsync(int id);
}
