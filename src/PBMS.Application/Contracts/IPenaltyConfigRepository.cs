using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

public interface IPenaltyConfigRepository : IRepository<PenaltyConfig>
{
    Task<PenaltyConfig?> GetActiveConfigByIncidentTypeAsync(int incidentTypeId);
}
