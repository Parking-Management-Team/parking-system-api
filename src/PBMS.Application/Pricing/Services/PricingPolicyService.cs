using PBMS.Application.Contracts;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;

namespace PBMS.Application.Pricing.Services;

/// <summary>
/// Triển khai nghiệp vụ quản lý Chính sách giá (IPricingPolicyService).
///
/// Lớp này chứa toàn bộ business logic liên quan đến PricingPolicy và PricingWindow:
///   - Tạo chính sách giá kèm khung giờ (Scenario 1)
///   - Truy vấn, cập nhật chính sách giá
///   - Quản lý khung giờ tính giá
///
/// Nguyên tắc: Service KHÔNG biết về EF Core hay Database.
///             Chỉ giao tiếp với DB thông qua IPricingPolicyRepository.
/// </summary>
public class PricingPolicyService : IPricingPolicyService
{
    private readonly IPricingPolicyRepository _policyRepository;
    private readonly IRepository<VehicleType> _vehicleTypeRepository;

    /// <summary>
    /// Constructor nhận repositories qua Dependency Injection.
    /// </summary>
    public PricingPolicyService(
        IPricingPolicyRepository policyRepository,
        IRepository<VehicleType> vehicleTypeRepository)
    {
        _policyRepository = policyRepository
            ?? throw new ArgumentNullException(nameof(policyRepository));
        _vehicleTypeRepository = vehicleTypeRepository
            ?? throw new ArgumentNullException(nameof(vehicleTypeRepository));
    }

    // -----------------------------------------------------------------------
    // [Scenario 1] TẠO CHÍNH SÁCH GIÁ MỚI
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tạo mới một PricingPolicy kèm danh sách PricingWindow.
    ///
    /// Luồng xử lý:
    ///   1. Kiểm tra VehicleTypeId tồn tại.
    ///   2. Validate EffectiveEnd (nếu có) > EffectiveStart.
    ///   3. Validate danh sách PricingWindows không rỗng + từng window hợp lệ.
    ///   4. Tạo PricingPolicy entity với trạng thái Active.
    ///   5. Tạo toàn bộ PricingWindow entities gắn vào chính sách.
    ///   6. Lưu DB → trả về DTO.
    /// </summary>
    public async Task<PricingPolicyDto> CreatePricingPolicyAsync(CreatePricingPolicyRequest request)
    {
        // Bước 1: Kiểm tra VehicleType tồn tại
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new DomainException(
                errorCode: "VEHICLE_TYPE_NOT_FOUND",
                message: $"Không tìm thấy loại phương tiện với ID={request.VehicleTypeId}."
            );
        }

        // Bước 2: Validate khoảng thời gian hiệu lực
        if (request.EffectiveEnd.HasValue && request.EffectiveEnd.Value <= request.EffectiveStart)
        {
            throw new DomainException(
                errorCode: "INVALID_EFFECTIVE_DATE_RANGE",
                message: "Ngày kết thúc hiệu lực (EffectiveEnd) phải sau ngày bắt đầu (EffectiveStart)."
            );
        }

        // Bước 3: Validate danh sách PricingWindows
        if (request.PricingWindows == null || !request.PricingWindows.Any())
        {
            throw new DomainException(
                errorCode: "PRICING_WINDOWS_REQUIRED",
                message: "Chính sách giá phải có ít nhất một khung giờ (PricingWindow)."
            );
        }

        foreach (var windowReq in request.PricingWindows)
        {
            ValidatePricingWindowParams(
                baseDuration: windowReq.BaseDurationMinutes,
                basePrice: windowReq.BasePrice,
                incrementBlock: windowReq.IncrementBlockMinutes,
                incrementPrice: windowReq.IncrementPrice,
                windowCap: windowReq.WindowCap,
                windowName: windowReq.WindowName
            );
        }

        // Bước 4: Tạo PricingPolicy entity
        var policy = new PricingPolicy
        {
            VehicleTypeId = request.VehicleTypeId,
            PolicyName = request.PolicyName.Trim(),
            EffectiveStart = request.EffectiveStart,
            EffectiveEnd = request.EffectiveEnd,
            PricingPolicyStatus = "Active"
        };

        // Bước 5: Tạo PricingWindow entities
        foreach (var windowReq in request.PricingWindows)
        {
            policy.PricingWindows.Add(new PricingWindow
            {
                WindowName = windowReq.WindowName.Trim(),
                StartTime = windowReq.StartTime,
                EndTime = windowReq.EndTime,
                BaseDurationMinutes = windowReq.BaseDurationMinutes,
                BasePrice = windowReq.BasePrice,
                IncrementBlockMinutes = windowReq.IncrementBlockMinutes,
                IncrementPrice = windowReq.IncrementPrice,
                WindowCap = windowReq.WindowCap,
                GracePeriodMinutes = windowReq.GracePeriodMinutes
            });
        }

        // Bước 6: Lưu vào database
        await _policyRepository.AddAsync(policy);
        await _policyRepository.SaveChangesAsync();

        // Trả về DTO (gắn thêm thông tin VehicleType để tiện hiển thị)
        return MapToDto(policy, vehicleType.TypeName);
    }

    // -----------------------------------------------------------------------
    // TRUY VẤN CHÍNH SÁCH GIÁ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy thông tin PricingPolicy theo ID kèm danh sách PricingWindows.
    /// </summary>
    public async Task<PricingPolicyDto> GetPricingPolicyByIdAsync(int id)
    {
        var policy = await _policyRepository.GetByIdWithWindowsAsync(id);
        if (policy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: $"Không tìm thấy chính sách giá với ID={id}."
            );
        }

        return MapToDto(policy, policy.VehicleType?.TypeName);
    }

    /// <summary>
    /// Lấy danh sách tất cả PricingPolicy, hỗ trợ lọc.
    /// </summary>
    public async Task<IEnumerable<PricingPolicyDto>> GetAllPricingPoliciesAsync(
        int? vehicleTypeId = null,
        string? status = null)
    {
        var policies = await _policyRepository.GetAllWithWindowsAsync(vehicleTypeId, status);
        return policies.Select(p => MapToDto(p, p.VehicleType?.TypeName));
    }

    // -----------------------------------------------------------------------
    // CẬP NHẬT CHÍNH SÁCH GIÁ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật partial PricingPolicy (chỉ cập nhật trường có giá trị).
    /// </summary>
    public async Task<PricingPolicyDto> UpdatePricingPolicyAsync(int id, UpdatePricingPolicyRequest request)
    {
        var policy = await _policyRepository.GetByIdWithWindowsAsync(id);
        if (policy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: $"Không tìm thấy chính sách giá với ID={id}."
            );
        }

        // Cập nhật các trường nếu được cung cấp
        if (request.PolicyName != null)
        {
            policy.PolicyName = request.PolicyName.Trim();
        }

        if (request.EffectiveStart.HasValue)
        {
            policy.EffectiveStart = request.EffectiveStart.Value;
        }

        if (request.EffectiveEnd.HasValue)
        {
            // Validate khoảng thời gian sau khi cập nhật
            var effectiveStart = request.EffectiveStart ?? policy.EffectiveStart;
            if (request.EffectiveEnd.Value <= effectiveStart)
            {
                throw new DomainException(
                    errorCode: "INVALID_EFFECTIVE_DATE_RANGE",
                    message: "Ngày kết thúc hiệu lực (EffectiveEnd) phải sau ngày bắt đầu (EffectiveStart)."
                );
            }
            policy.EffectiveEnd = request.EffectiveEnd.Value;
        }

        if (request.PricingPolicyStatus != null)
        {
            var validStatuses = new[] { "Active", "Inactive", "Expired" };
            if (!validStatuses.Contains(request.PricingPolicyStatus))
            {
                throw new DomainException(
                    errorCode: "INVALID_PRICING_POLICY_STATUS",
                    message: $"Trạng thái '{request.PricingPolicyStatus}' không hợp lệ. Các giá trị hợp lệ: Active, Inactive, Expired."
                );
            }
            policy.PricingPolicyStatus = request.PricingPolicyStatus;
        }

        _policyRepository.Update(policy);
        await _policyRepository.SaveChangesAsync();

        return MapToDto(policy, policy.VehicleType?.TypeName);
    }

    // -----------------------------------------------------------------------
    // QUẢN LÝ KHUNG GIỜ (PricingWindow)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thêm một PricingWindow mới vào chính sách giá đã tồn tại.
    /// </summary>
    public async Task<PricingWindowDto> AddPricingWindowAsync(
        int pricingPolicyId,
        CreatePricingWindowRequest request)
    {
        // Kiểm tra chính sách tồn tại
        var policy = await _policyRepository.GetByIdAsync(pricingPolicyId);
        if (policy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: $"Không tìm thấy chính sách giá với ID={pricingPolicyId}."
            );
        }

        // Validate tham số khung giờ
        ValidatePricingWindowParams(
            baseDuration: request.BaseDurationMinutes,
            basePrice: request.BasePrice,
            incrementBlock: request.IncrementBlockMinutes,
            incrementPrice: request.IncrementPrice,
            windowCap: request.WindowCap,
            windowName: request.WindowName
        );

        var window = new PricingWindow
        {
            PricingPolicyId = pricingPolicyId,
            WindowName = request.WindowName.Trim(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            BaseDurationMinutes = request.BaseDurationMinutes,
            BasePrice = request.BasePrice,
            IncrementBlockMinutes = request.IncrementBlockMinutes,
            IncrementPrice = request.IncrementPrice,
            WindowCap = request.WindowCap,
            GracePeriodMinutes = request.GracePeriodMinutes
        };

        await _policyRepository.AddWindowAsync(window);
        await _policyRepository.SaveChangesAsync();

        return MapWindowToDto(window);
    }

    /// <summary>
    /// Cập nhật partial PricingWindow (chỉ cập nhật trường có giá trị).
    /// </summary>
    public async Task<PricingWindowDto> UpdatePricingWindowAsync(
        int pricingWindowId,
        UpdatePricingWindowRequest request)
    {
        var window = await _policyRepository.GetWindowByIdAsync(pricingWindowId);
        if (window == null)
        {
            throw new DomainException(
                errorCode: "PRICING_WINDOW_NOT_FOUND",
                message: $"Không tìm thấy khung giờ với ID={pricingWindowId}."
            );
        }

        // Cập nhật partial
        if (request.WindowName != null) window.WindowName = request.WindowName.Trim();
        if (request.StartTime.HasValue) window.StartTime = request.StartTime.Value;
        if (request.EndTime.HasValue) window.EndTime = request.EndTime.Value;
        if (request.BaseDurationMinutes.HasValue) window.BaseDurationMinutes = request.BaseDurationMinutes.Value;
        if (request.BasePrice.HasValue) window.BasePrice = request.BasePrice.Value;
        if (request.IncrementBlockMinutes.HasValue) window.IncrementBlockMinutes = request.IncrementBlockMinutes.Value;
        if (request.IncrementPrice.HasValue) window.IncrementPrice = request.IncrementPrice.Value;
        if (request.GracePeriodMinutes.HasValue) window.GracePeriodMinutes = request.GracePeriodMinutes.Value;

        // Xử lý WindowCap
        if (request.RemoveWindowCap)
        {
            window.WindowCap = null;
        }
        else if (request.WindowCap.HasValue)
        {
            window.WindowCap = request.WindowCap.Value;
        }

        // Validate sau khi cập nhật
        ValidatePricingWindowParams(
            baseDuration: window.BaseDurationMinutes,
            basePrice: window.BasePrice,
            incrementBlock: window.IncrementBlockMinutes,
            incrementPrice: window.IncrementPrice,
            windowCap: window.WindowCap,
            windowName: window.WindowName
        );

        _policyRepository.UpdateWindow(window);
        await _policyRepository.SaveChangesAsync();

        return MapWindowToDto(window);
    }

    /// <summary>
    /// Xóa PricingWindow (không cho phép nếu là window cuối cùng của chính sách).
    /// </summary>
    public async Task DeletePricingWindowAsync(int pricingWindowId)
    {
        var window = await _policyRepository.GetWindowByIdAsync(pricingWindowId);
        if (window == null)
        {
            throw new DomainException(
                errorCode: "PRICING_WINDOW_NOT_FOUND",
                message: $"Không tìm thấy khung giờ với ID={pricingWindowId}."
            );
        }

        // Kiểm tra số window còn lại của chính sách
        var windowCount = await _policyRepository.CountWindowsByPolicyIdAsync(window.PricingPolicyId);
        if (windowCount <= 1)
        {
            throw new DomainException(
                errorCode: "CANNOT_DELETE_LAST_PRICING_WINDOW",
                message: "Không thể xóa khung giờ cuối cùng của chính sách giá. " +
                         "Mỗi chính sách phải có ít nhất một khung giờ."
            );
        }

        await _policyRepository.RemoveWindowAsync(window);
        await _policyRepository.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // PRIVATE HELPERS
    // -----------------------------------------------------------------------

    /// <summary>
    /// Validate các tham số cấu hình của PricingWindow.
    /// Theo các constraint trong SRS (8.3.3 Physical Tables - pricing_window):
    ///   base_duration_minutes > 0
    ///   increment_block_minutes > 0
    ///   base_price >= 0
    ///   increment_price >= 0
    ///   window_cap null hoặc >= base_price
    /// </summary>
    private static void ValidatePricingWindowParams(
        int baseDuration,
        decimal basePrice,
        int incrementBlock,
        decimal incrementPrice,
        decimal? windowCap,
        string windowName)
    {
        if (baseDuration <= 0)
        {
            throw new DomainException(
                errorCode: "INVALID_BASE_DURATION",
                message: $"[{windowName}] BaseDurationMinutes phải lớn hơn 0."
            );
        }

        if (basePrice < 0)
        {
            throw new DomainException(
                errorCode: "INVALID_BASE_PRICE",
                message: $"[{windowName}] BasePrice phải >= 0."
            );
        }

        if (incrementBlock <= 0)
        {
            throw new DomainException(
                errorCode: "INVALID_INCREMENT_BLOCK",
                message: $"[{windowName}] IncrementBlockMinutes phải lớn hơn 0."
            );
        }

        if (incrementPrice < 0)
        {
            throw new DomainException(
                errorCode: "INVALID_INCREMENT_PRICE",
                message: $"[{windowName}] IncrementPrice phải >= 0."
            );
        }

        if (windowCap.HasValue && windowCap.Value < basePrice)
        {
            throw new DomainException(
                errorCode: "WINDOW_CAP_BELOW_BASE_PRICE",
                message: $"[{windowName}] WindowCap ({windowCap.Value:N0}) phải >= BasePrice ({basePrice:N0})."
            );
        }
    }

    /// <summary>
    /// Map PricingPolicy entity sang PricingPolicyDto.
    /// </summary>
    private static PricingPolicyDto MapToDto(PricingPolicy policy, string? vehicleTypeName)
    {
        return new PricingPolicyDto
        {
            Id = policy.Id,
            VehicleTypeId = policy.VehicleTypeId,
            VehicleTypeName = vehicleTypeName,
            PolicyName = policy.PolicyName,
            EffectiveStart = policy.EffectiveStart,
            EffectiveEnd = policy.EffectiveEnd,
            PricingPolicyStatus = policy.PricingPolicyStatus,
            CreatedAt = policy.CreatedAt,
            PricingWindows = policy.PricingWindows.Select(MapWindowToDto).ToList()
        };
    }

    /// <summary>
    /// Map PricingWindow entity sang PricingWindowDto.
    /// </summary>
    private static PricingWindowDto MapWindowToDto(PricingWindow window)
    {
        return new PricingWindowDto
        {
            Id = window.Id,
            PricingPolicyId = window.PricingPolicyId,
            WindowName = window.WindowName,
            StartTime = window.StartTime,
            EndTime = window.EndTime,
            BaseDurationMinutes = window.BaseDurationMinutes,
            BasePrice = window.BasePrice,
            IncrementBlockMinutes = window.IncrementBlockMinutes,
            IncrementPrice = window.IncrementPrice,
            WindowCap = window.WindowCap,
            GracePeriodMinutes = window.GracePeriodMinutes,
            CreatedAt = window.CreatedAt
        };
    }
}
