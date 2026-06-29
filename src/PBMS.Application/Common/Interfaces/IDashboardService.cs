using System.Threading.Tasks;
using PBMS.Application.Common;
using PBMS.Application.Common.DTOs;

namespace PBMS.Application.Common.Interfaces;

public interface IDashboardService
{
    Task<BaseResponse<DashboardSummaryDto>> GetSummaryAsync();
}
