using PBMS.Application.Contracts;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;

namespace PBMS.Application.Pricing.Services;

/// <summary>
/// Triển khai nghiệp vụ quản lý Chính sách giá (IPricingPolicyService).
///
/// Business Rules được triển khai:
///   BR-FEE-024 : Check-in chỉ chấp nhận khi có đúng 1 Policy hợp lệ cho VehicleType.
///   BR-FEE-025 : Policy cùng VehicleType không overlap khoảng effective_start–effective_end.
///   BR-FEE-027 : PricingWindow trong cùng Policy phải phủ đủ 24h, không overlap.
///   BR-FEE-028 : Không kích hoạt Policy nếu Window overlap hoặc không phủ đủ 24h.
///   BR-FEE-029 : Policy đã ACTIVE không được sửa cấu hình giá (Window, BasePrice, v.v.).
///   BR-FEE-031 : effective_start của Policy ACTIVE không được sửa.
///   BR-FEE-032 : effective_end của Policy ACTIVE/tương lai có thể sửa theo điều kiện.
///
/// Nguyên tắc: Service KHÔNG biết về EF Core hay Database.
///             Chỉ giao tiếp với DB thông qua IPricingPolicyRepository.
/// </summary>
public class PricingPolicyService : IPricingPolicyService
{
    private readonly IPricingPolicyRepository _policyRepository;
    private readonly IRepository<VehicleType> _vehicleTypeRepository;

    // Các trạng thái hợp lệ của PricingPolicy
    private static readonly string[] ValidStatuses = { "Active", "Inactive", "Expired" };

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
    // [Scenario 1 - Bước 1/2] TẠO CHÍNH SÁCH GIÁ MỚI (status = INACTIVE)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tạo mới một PricingPolicy kèm danh sách PricingWindow với trạng thái INACTIVE.
    /// Phải gọi ActivatePricingPolicyAsync để kích hoạt sau khi cấu hình xong.
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

        // Bước 3: Validate danh sách PricingWindows không rỗng
        if (request.PricingWindows == null || !request.PricingWindows.Any())
        {
            throw new DomainException(
                errorCode: "PRICING_WINDOWS_REQUIRED",
                message: "Chính sách giá phải có ít nhất một khung giờ (PricingWindow)."
            );
        }

        // Bước 4: Validate params của từng PricingWindow
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

        // Bước 5: Tạo PricingPolicy với status INACTIVE
        // Policy sẽ được kích hoạt riêng qua ActivatePricingPolicyAsync (BR-FEE-028)
        var policy = new PricingPolicy
        {
            VehicleTypeId = request.VehicleTypeId,
            PolicyName = request.PolicyName.Trim(),
            EffectiveStart = request.EffectiveStart.Date,
            EffectiveEnd = request.EffectiveEnd.HasValue ? request.EffectiveEnd.Value.Date : null,
            PricingPolicyStatus = "Inactive"
        };

        // Bước 6: Tạo PricingWindow entities gắn vào policy
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

        // Bước 7: Lưu vào database
        await _policyRepository.AddAsync(policy);
        await _policyRepository.SaveChangesAsync();

        return MapToDto(policy, vehicleType.TypeName);
    }

    // -----------------------------------------------------------------------
    // [Scenario 1 - Bước 2/2] KÍCH HOẠT CHÍNH SÁCH GIÁ (INACTIVE → ACTIVE)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Kích hoạt PricingPolicy từ INACTIVE → ACTIVE.
    ///
    /// Validate đầy đủ trước khi kích hoạt:
    ///   1. BR-FEE-027/028: Windows phủ đủ 24h và không overlap nhau.
    ///   2. BR-FEE-025: Không overlap với Policy khác cùng VehicleType.
    ///   3. Chuyển trạng thái → ACTIVE.
    /// </summary>
    public async Task<PricingPolicyDto> ActivatePricingPolicyAsync(int id)
    {
        var policy = await _policyRepository.GetByIdWithWindowsAsync(id);
        if (policy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: $"Không tìm thấy chính sách giá với ID={id}."
            );
        }

        // Chỉ INACTIVE mới được kích hoạt
        if (policy.PricingPolicyStatus != "Inactive")
        {
            throw new DomainException(
                errorCode: "POLICY_ALREADY_ACTIVE_OR_EXPIRED",
                message: $"Chỉ có thể kích hoạt chính sách ở trạng thái Inactive. " +
                         $"Trạng thái hiện tại: {policy.PricingPolicyStatus}."
            );
        }

        // BR-FEE-027/028: Validate windows phủ đủ 24h và không overlap
        var windows = policy.PricingWindows.ToList();
        ValidateWindowsCover24Hours(windows, policy.PolicyName);

        // BR-FEE-025: Validate không overlap với policy khác cùng VehicleType
        var hasOverlap = await _policyRepository.HasOverlapPolicyAsync(
            vehicleTypeId: policy.VehicleTypeId,
            effectiveStart: policy.EffectiveStart,
            effectiveEnd: policy.EffectiveEnd,
            excludePolicyId: policy.Id
        );

        if (hasOverlap)
        {
            throw new DomainException(
                errorCode: "POLICY_OVERLAP",
                message: "Đã tồn tại chính sách giá Active/Inactive khác cho cùng loại xe " +
                         "với khoảng thời gian hiệu lực chồng nhau. " +
                         "Vui lòng điều chỉnh EffectiveStart/EffectiveEnd trước khi kích hoạt."
            );
        }

        // Kích hoạt chính sách
        policy.PricingPolicyStatus = "Active";
        _policyRepository.Update(policy);
        await _policyRepository.SaveChangesAsync();

        return MapToDto(policy, policy.VehicleType?.TypeName);
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
    /// Cập nhật partial PricingPolicy.
    ///
    /// Áp dụng đầy đủ business rules:
    ///   BR-FEE-029: Policy ACTIVE bị khóa, không sửa được effective_start hay pricing config.
    ///   BR-FEE-031: effective_start của Policy ACTIVE không được sửa.
    ///   BR-FEE-032: effective_end có thể sửa với điều kiện: trong tương lai, không overlap,
    ///               không làm mất hiệu lực hồi tố, Policy EXPIRED không được sửa.
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

        var isActive = policy.PricingPolicyStatus == "Active";
        var isExpired = policy.PricingPolicyStatus == "Expired";

        // BR-FEE-031: effective_start của Policy ACTIVE không được sửa
        if (isActive && request.EffectiveStart.HasValue)
        {
            throw new DomainException(
                errorCode: "CANNOT_MODIFY_ACTIVE_POLICY_START_DATE",
                message: "Không được sửa ngày bắt đầu hiệu lực (EffectiveStart) khi chính sách đã ở trạng thái Active. (BR-FEE-031)"
            );
        }

        // BR-FEE-032: Policy EXPIRED không được sửa EffectiveEnd
        if (isExpired && request.EffectiveEnd.HasValue)
        {
            throw new DomainException(
                errorCode: "CANNOT_MODIFY_EXPIRED_POLICY",
                message: "Không được sửa ngày kết thúc hiệu lực của chính sách đã Expired. (BR-FEE-032)"
            );
        }

        // Cập nhật PolicyName (không bị khóa khi ACTIVE)
        if (request.PolicyName != null)
        {
            policy.PolicyName = request.PolicyName.Trim();
        }

        // Cập nhật EffectiveStart (chỉ khi INACTIVE)
        if (request.EffectiveStart.HasValue)
        {
            policy.EffectiveStart = request.EffectiveStart.Value.Date;
        }

        // Cập nhật EffectiveEnd
        if (request.EffectiveEnd.HasValue)
        {
            var effectiveStart = policy.EffectiveStart;
            if (request.EffectiveEnd.Value <= effectiveStart)
            {
                throw new DomainException(
                    errorCode: "INVALID_EFFECTIVE_DATE_RANGE",
                    message: "Ngày kết thúc hiệu lực (EffectiveEnd) phải sau ngày bắt đầu (EffectiveStart)."
                );
            }
            policy.EffectiveEnd = request.EffectiveEnd.Value.Date;
        }

        // Cập nhật trạng thái (chỉ các giá trị hợp lệ)
        if (request.PricingPolicyStatus != null)
        {
            if (!ValidStatuses.Contains(request.PricingPolicyStatus))
            {
                throw new DomainException(
                    errorCode: "INVALID_PRICING_POLICY_STATUS",
                    message: $"Trạng thái '{request.PricingPolicyStatus}' không hợp lệ. " +
                             $"Các giá trị hợp lệ: {string.Join(", ", ValidStatuses)}."
                );
            }
            // Không cho phép chuyển ngược về Inactive từ Active
            if (isActive && request.PricingPolicyStatus == "Inactive")
            {
                throw new DomainException(
                    errorCode: "CANNOT_DEACTIVATE_ACTIVE_POLICY",
                    message: "Không thể chuyển chính sách đang Active về Inactive. " +
                             "Chỉ có thể chuyển sang Expired."
                );
            }
            policy.PricingPolicyStatus = request.PricingPolicyStatus;
        }

        _policyRepository.Update(policy);
        await _policyRepository.SaveChangesAsync();

        return MapToDto(policy, policy.VehicleType?.TypeName);
    }

    // -----------------------------------------------------------------------
    // QUẢN LÝ KHUNG GIỜ (PricingWindow) — Bị khóa khi Policy đã ACTIVE
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thêm một PricingWindow mới vào chính sách giá đã tồn tại.
    /// BR-FEE-029: Bị khóa khi Policy đã ACTIVE.
    /// </summary>
    public async Task<PricingWindowDto> AddPricingWindowAsync(
        int pricingPolicyId,
        CreatePricingWindowRequest request)
    {
        var policy = await _policyRepository.GetByIdAsync(pricingPolicyId);
        if (policy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: $"Không tìm thấy chính sách giá với ID={pricingPolicyId}."
            );
        }

        // BR-FEE-029: Không cho sửa cấu hình giá khi Policy đã ACTIVE
        GuardAgainstModifyingActivePolicy(policy);

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
    /// Cập nhật partial PricingWindow.
    /// BR-FEE-029: Bị khóa khi Policy đã ACTIVE.
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

        // BR-FEE-029: Không cho sửa cấu hình giá khi Policy đã ACTIVE
        if (window.PricingPolicy != null)
        {
            GuardAgainstModifyingActivePolicy(window.PricingPolicy);
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
    /// BR-FEE-029: Bị khóa khi Policy đã ACTIVE.
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

        // BR-FEE-029: Không cho sửa cấu hình giá khi Policy đã ACTIVE
        if (window.PricingPolicy != null)
        {
            GuardAgainstModifyingActivePolicy(window.PricingPolicy);
        }

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
    /// Guard: ném DomainException nếu Policy đang ở trạng thái Active.
    /// Dùng để chặn mọi thay đổi cấu hình giá khi Policy đã Active (BR-FEE-029).
    /// </summary>
    private static void GuardAgainstModifyingActivePolicy(PricingPolicy policy)
    {
        if (policy.PricingPolicyStatus == "Active")
        {
            throw new DomainException(
                errorCode: "CANNOT_MODIFY_ACTIVE_POLICY",
                message: $"Không được sửa cấu hình giá của chính sách đang ở trạng thái Active (ID={policy.Id}). " +
                         "Theo BR-FEE-029: Policy đã ACTIVE hoặc đã từng được dùng để chấp nhận " +
                         "Parking Session không được sửa cấu hình giá."
            );
        }
    }

    /// <summary>
    /// Validate các PricingWindow trong cùng Policy phủ đủ 24h và không overlap.
    ///
    /// Theo BR-FEE-027/028:
    ///   - Tất cả Window phải không overlap nhau.
    ///   - Tổng coverage của tất cả Window phải đúng bằng 24h (= 1440 phút).
    ///   - Window qua đêm: EndTime &lt; StartTime (ví dụ: 22:00 → 06:00).
    ///   - Thời điểm bắt đầu thuộc Window đó; thời điểm kết thúc thuộc Window tiếp theo.
    /// </summary>
    private static void ValidateWindowsCover24Hours(List<PricingWindow> windows, string policyName)
    {
        if (windows.Count == 0)
        {
            throw new DomainException(
                errorCode: "PRICING_WINDOWS_REQUIRED",
                message: $"[{policyName}] Chính sách giá phải có ít nhất một khung giờ."
            );
        }

        // Chuyển mỗi window thành danh sách khoảng [startMinutes, endMinutes) trong ngày 24h
        // startMinutes và endMinutes tính từ 00:00 (phút)
        // Window qua đêm (EndTime < StartTime) được xử lý thành [start, 1440) + [0, end)
        var segments = new List<(int Start, int End, string Name)>();

        foreach (var w in windows)
        {
            int startMin = (int)w.StartTime.TotalMinutes;
            int endMin = (int)w.EndTime.TotalMinutes;

            if (startMin == endMin)
            {
                // Window chiếm đúng 24h (StartTime == EndTime → cả ngày)
                segments.Add((0, 1440, w.WindowName));
            }
            else if (endMin > startMin)
            {
                // Window trong ngày bình thường
                segments.Add((startMin, endMin, w.WindowName));
            }
            else
            {
                // Window qua đêm: chia thành 2 đoạn
                segments.Add((startMin, 1440, w.WindowName));
                segments.Add((0, endMin, w.WindowName));
            }
        }

        // Sắp xếp theo thời gian bắt đầu
        segments.Sort((a, b) => a.Start.CompareTo(b.Start));

        // Kiểm tra overlap và phủ đủ 24h
        int coveredMinutes = 0;
        int expectedStart = 0;

        foreach (var seg in segments)
        {
            if (seg.Start < expectedStart)
            {
                // Overlap: segment bắt đầu trước khi segment trước kết thúc
                throw new DomainException(
                    errorCode: "WINDOWS_OVERLAP",
                    message: $"[{policyName}] Khung giờ '{seg.Name}' bị chồng chéo với khung giờ khác. " +
                             "Các khung giờ không được overlap nhau. (BR-FEE-027)"
                );
            }

            if (seg.Start > expectedStart)
            {
                // Có khoảng trống (gap) — không phủ đủ 24h
                throw new DomainException(
                    errorCode: "WINDOWS_DO_NOT_COVER_24_HOURS",
                    message: $"[{policyName}] Có khoảng thời gian không thuộc khung giờ nào " +
                             $"({TimeSpan.FromMinutes(expectedStart):hh\\:mm}–{TimeSpan.FromMinutes(seg.Start):hh\\:mm}). " +
                             "Tất cả khung giờ phải phủ đủ 24h. (BR-FEE-027)"
                );
            }

            coveredMinutes += seg.End - seg.Start;
            expectedStart = seg.End;
        }

        // Kiểm tra coverage cuối cùng phải đúng 1440 phút
        if (expectedStart != 1440 || coveredMinutes != 1440)
        {
            throw new DomainException(
                errorCode: "WINDOWS_DO_NOT_COVER_24_HOURS",
                message: $"[{policyName}] Các khung giờ chỉ phủ {coveredMinutes} phút, " +
                         "cần phủ đủ 1440 phút (24 giờ). (BR-FEE-027)"
            );
        }
    }

    /// <summary>
    /// Validate các tham số cấu hình của PricingWindow.
    /// Theo các constraint trong SRS §8.3.3.19 (pricing_window):
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
