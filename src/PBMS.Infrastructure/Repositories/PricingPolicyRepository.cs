using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Triển khai IPricingPolicyRepository — truy vấn và lưu trữ PricingPolicy và PricingWindow.
/// Kế thừa BaseRepository&lt;PricingPolicy&gt; để dùng lại CRUD cơ bản.
/// </summary>
public class PricingPolicyRepository : BaseRepository<PricingPolicy>, IPricingPolicyRepository
{
    private readonly DbSet<PricingWindow> _windowDbSet;

    /// <summary>
    /// Constructor nhận AppDbContext qua Dependency Injection.
    /// </summary>
    public PricingPolicyRepository(AppDbContext context) : base(context)
    {
        _windowDbSet = context.PricingWindows;
    }

    // -----------------------------------------------------------------------
    // PRICING POLICY QUERIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy PricingPolicy theo ID, eager load PricingWindows và VehicleType.
    /// </summary>
    public async Task<PricingPolicy?> GetByIdWithWindowsAsync(int id)
    {
        return await _dbSet
            .Include(pp => pp.PricingWindows)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.BasePricingRuleConfig)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.IncrementPricingRuleConfig)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.DailyCapRuleConfig)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.GracePeriodRuleConfig)
            .Include(pp => pp.VehicleType)
            .FirstOrDefaultAsync(pp => pp.Id == id);
    }

    /// <summary>
    /// Lấy toàn bộ danh sách PricingPolicy với eager load PricingWindows và VehicleType.
    /// Hỗ trợ lọc theo vehicleTypeId và status.
    /// </summary>
    public async Task<IEnumerable<PricingPolicy>> GetAllWithWindowsAsync(
        int? vehicleTypeId = null,
        string? status = null)
    {
        var query = _dbSet
            .Include(pp => pp.PricingWindows)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.BasePricingRuleConfig)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.IncrementPricingRuleConfig)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.DailyCapRuleConfig)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.GracePeriodRuleConfig)
            .Include(pp => pp.VehicleType)
            .AsQueryable();

        if (vehicleTypeId.HasValue)
        {
            query = query.Where(pp => pp.VehicleTypeId == vehicleTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(pp => pp.PricingPolicyStatus == status);
        }

        return await query
            .OrderByDescending(pp => pp.EffectiveStart)
            .ToListAsync();
    }

    /// <summary>
    /// Tìm PricingPolicy đang Active cho loại xe cụ thể tại thời điểm cho trước.
    ///
    /// Điều kiện Active:
    ///   - PricingPolicyStatus == "Active"
    ///   - VehicleTypeId trùng khớp
    ///   - EffectiveStart &lt;= atTime
    ///   - EffectiveEnd IS NULL hoặc EffectiveEnd > atTime
    ///
    /// Kết quả: lấy policy mới nhất (EffectiveStart lớn nhất) nếu có nhiều hơn 1.
    /// </summary>
    public async Task<PricingPolicy?> GetActivePolicyAsync(int vehicleTypeId, DateTime atTime)
    {
        return await _dbSet
            .Include(pp => pp.PricingWindows)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.BasePricingRuleConfig)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.IncrementPricingRuleConfig)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.DailyCapRuleConfig)
            .Include(pp => pp.PricingRules)
                .ThenInclude(r => r.GracePeriodRuleConfig)
            .Where(pp =>
                pp.VehicleTypeId == vehicleTypeId &&
                pp.PricingPolicyStatus == "Active" &&
                pp.EffectiveStart <= atTime.Date &&
                (pp.EffectiveEnd == null || pp.EffectiveEnd.Value >= atTime.Date)
            )
            .OrderByDescending(pp => pp.EffectiveStart)
            .FirstOrDefaultAsync();
    }

    // -----------------------------------------------------------------------
    // PRICING WINDOW OPERATIONS
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy PricingWindow theo ID, kèm thông tin PricingPolicy cha.
    /// </summary>
    public async Task<PricingWindow?> GetWindowByIdAsync(int pricingWindowId)
    {
        return await _windowDbSet
            .Include(pw => pw.PricingPolicy)
            .FirstOrDefaultAsync(pw => pw.Id == pricingWindowId);
    }

    /// <summary>
    /// Thêm mới một PricingWindow vào database.
    /// </summary>
    public async Task AddWindowAsync(PricingWindow window)
    {
        await _windowDbSet.AddAsync(window);
    }

    /// <summary>
    /// Đánh dấu cập nhật một PricingWindow (EF Core sinh UPDATE khi SaveChanges).
    /// </summary>
    public void UpdateWindow(PricingWindow window)
    {
        _windowDbSet.Update(window);
    }

    /// <summary>
    /// Xóa một PricingWindow khỏi database.
    /// </summary>
    public async Task RemoveWindowAsync(PricingWindow window)
    {
        _windowDbSet.Remove(window);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Đếm số PricingWindow của một PricingPolicy.
    /// Dùng để ngăn xóa window cuối cùng.
    /// </summary>
    public async Task<int> CountWindowsByPolicyIdAsync(int pricingPolicyId)
    {
        return await _windowDbSet.CountAsync(pw => pw.PricingPolicyId == pricingPolicyId);
    }

    /// <summary>
    /// Kiểm tra xem có Policy nào cùng VehicleType overlap với khoảng thời gian không.
    ///
    /// Hai khoảng [A_start, A_end] và [B_start, B_end] overlap khi:
    ///   A_start &lt; B_end AND B_start &lt; A_end
    /// Với null-end nghĩa là vô thời hạn (dùng DateTime.MaxValue để so sánh).
    ///
    /// Theo BR-FEE-025: thời điểm kết thúc Policy trước có thể bằng thời điểm bắt đầu
    /// Policy sau — khi đó tại thời điểm chuyển tiếp chỉ Policy mới áp dụng.
    /// Vì vậy dùng điều kiện strict &lt; thay vì &lt;=.
    /// </summary>
    public async Task<bool> HasOverlapPolicyAsync(
        int vehicleTypeId,
        DateTime effectiveStart,
        DateTime? effectiveEnd,
        int? excludePolicyId = null)
    {
        // Chuẩn hóa: null end = vô thời hạn → DateTime.MaxValue
        var newEnd = effectiveEnd ?? DateTime.MaxValue;

        var query = _dbSet.Where(pp =>
            pp.VehicleTypeId == vehicleTypeId &&
            pp.PricingPolicyStatus != "Expired" &&
            // Loại trừ policy đang được update (nếu có)
            (excludePolicyId == null || pp.Id != excludePolicyId.Value) &&
            // Overlap: newStart < existEnd AND existStart < newEnd
            pp.EffectiveStart < newEnd &&
            (pp.EffectiveEnd == null || pp.EffectiveEnd.Value > effectiveStart)
        );

        return await query.AnyAsync();
    }
}
