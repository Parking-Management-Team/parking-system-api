using PBMS.Application.AuditLog.DTOs;
using PBMS.Application.Common;

namespace PBMS.Application.AuditLog.Interfaces;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        int pageIndex,
        int pageSize,
        int? accountId = null,
        string? action = null,
        string? targetTable = null);

    Task<AuditLogDto?> GetAuditLogByIdAsync(int id);

    Task LogAsync(int? accountId, string action, string? targetTable, int? targetId, string? description);
}