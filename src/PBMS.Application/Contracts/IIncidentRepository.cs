using PBMS.Application.Common;
using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

public interface IIncidentRepository : IRepository<PBMS.Domain.Entities.Incident>
{
    Task<PBMS.Domain.Entities.Incident?> GetIncidentWithDetailsAsync(int id);
    Task<PagedResult<PBMS.Domain.Entities.Incident>> GetPagedIncidentsWithDetailsAsync(int pageIndex, int pageSize);
    Task<IEnumerable<PBMS.Domain.Entities.Incident>> GetIncidentsBySessionWithDetailsAsync(int sessionId);
}
