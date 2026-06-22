using System.Collections.Generic;
using System.Threading.Tasks;
using PBMS.Application.Incident.DTOs;

namespace PBMS.Application.Incident.Interfaces;

public interface IPenaltyConfigService
{
    Task<IEnumerable<PenaltyConfigDto>> GetAllConfigsAsync(int? incidentTypeId, bool? onlyActive);
    Task<PenaltyConfigDto?> GetActiveConfigByIncidentTypeAsync(int incidentTypeId);
    Task<PenaltyConfigDto> CreateConfigAsync(CreatePenaltyConfigRequest request);
    Task<bool> DeactivateConfigAsync(int id);
    Task<bool> DeleteConfigAsync(int id);
}
