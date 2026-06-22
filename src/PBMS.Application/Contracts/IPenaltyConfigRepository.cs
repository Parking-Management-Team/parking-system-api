using System.Collections.Generic;
using System.Threading.Tasks;
using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

public interface IPenaltyConfigRepository : IRepository<PenaltyConfig>
{
    Task<PenaltyConfig?> GetActiveConfigByIncidentTypeAsync(int incidentTypeId);
    Task<IEnumerable<PenaltyConfig>> GetAllConfigsWithDetailsAsync(int? incidentTypeId, bool? onlyActive);
}
