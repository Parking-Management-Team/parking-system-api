using AutoMapper;
using PBMS.Application.AuditLog.DTOs;
using PBMS.Application.AuditLog.Interfaces;
using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;

namespace PBMS.Application.AuditLog.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IRepository<PBMS.Domain.Entities.AuditLog> _auditLogRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IMapper _mapper;

    public AuditLogService(
        IRepository<PBMS.Domain.Entities.AuditLog> auditLogRepository,
        IRepository<Account> accountRepository,
        IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        int pageIndex,
        int pageSize,
        int? accountId = null,
        string? action = null,
        string? targetTable = null)
    {
        var query = await _auditLogRepository.GetAllAsync();

        if (accountId.HasValue)
            query = query.Where(x => x.AccountId == accountId.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action.ToUpper() == action.ToUpper());

        if (!string.IsNullOrWhiteSpace(targetTable))
            query = query.Where(x => x.TargetTable != null && x.TargetTable.ToUpper() == targetTable.ToUpper());

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(items);

        return new PagedResult<AuditLogDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(int id)
    {
        var auditLog = await _auditLogRepository.GetByIdAsync(id);
        if (auditLog == null) return null;

        return _mapper.Map<AuditLogDto>(auditLog);
    }
}