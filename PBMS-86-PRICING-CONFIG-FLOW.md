# PBMS-86 — PS06: PRICING CONFIG
## Tài liệu luồng chạy tính năng

> **Ticket:** PBMS-86 | **Branch:** `feature/pricing-config`  
> **Actor:** Parking Manager  
> **Mục tiêu:** Cấu hình bảng giá linh hoạt theo loại xe và khung giờ, hệ thống tự động tính phí khi xe check-out.

---

## 1. Tổng quan kiến trúc (Clean Architecture)

```
HTTP Request
    │
    ▼
┌─────────────────────────────────────┐
│           PBMS.API                  │
│   PricingPoliciesController         │  ← Tiếp nhận request, trả response
└──────────────┬──────────────────────┘
               │ gọi Interface
               ▼
┌─────────────────────────────────────┐
│        PBMS.Application             │
│  IPricingPolicyService              │  ← Contract nghiệp vụ
│  PricingPolicyService               │  ← Xử lý nghiệp vụ, validate
│                                     │
│  IFeeCalculationService             │  ← Contract tính phí
│  FeeCalculationService              │  ← Core logic tính phí
└──────────────┬──────────────────────┘
               │ gọi Interface
               ▼
┌─────────────────────────────────────┐
│       PBMS.Infrastructure           │
│  IPricingPolicyRepository           │  ← Contract truy vấn DB
│  PricingPolicyRepository            │  ← EF Core implementation
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│          Database (PostgreSQL)      │
│  pricing_policy                     │
│  pricing_window                     │
└─────────────────────────────────────┘
```

---

## 2. Sơ đồ các Object được tạo

### 2.1 Domain Layer — Entities (đã có sẵn)

```
PBMS.Domain/Entities/
├── PricingPolicy.cs          ← Chính sách giá (1 policy → nhiều window)
│   ├── Id                    (PK)
│   ├── VehicleTypeId         (FK → VehicleType)
│   ├── PolicyName
│   ├── EffectiveStart        (date)
│   ├── EffectiveEnd          (date, nullable)
│   ├── PricingPolicyStatus   ("Active" / "Inactive" / "Expired")
│   └── PricingWindows        (Navigation → List<PricingWindow>)
│
└── PricingWindow.cs          ← Khung giờ tính giá
    ├── Id                    (PK)
    ├── PricingPolicyId       (FK → PricingPolicy)
    ├── WindowName
    ├── StartTime             (TimeSpan)
    ├── EndTime               (TimeSpan)
    ├── BaseDurationMinutes   (phút block đầu tiên)
    ├── BasePrice             (giá block đầu tiên)
    ├── IncrementBlockMinutes (kích thước block lũy tiến)
    ├── IncrementPrice        (giá mỗi block lũy tiến)
    ├── WindowCap             (mức trần, nullable)
    └── GracePeriodMinutes    (thời gian ân hạn)
```

### 2.2 Application Layer — DTOs

```
PBMS.Application/Pricing/DTOs/
│
├── PricingPolicyDto.cs           ← Trả về cho Client
│   ├── Id, VehicleTypeId, VehicleTypeName
│   ├── PolicyName, EffectiveStart, EffectiveEnd
│   ├── PricingPolicyStatus, CreatedAt
│   └── PricingWindows: List<PricingWindowDto>
│
├── PricingWindowDto.cs           ← Trả về cho Client
│   ├── Id, PricingPolicyId, WindowName
│   ├── StartTime, EndTime
│   ├── BaseDurationMinutes, BasePrice
│   ├── IncrementBlockMinutes, IncrementPrice
│   ├── WindowCap, GracePeriodMinutes, CreatedAt
│
├── CreatePricingPolicyRequest.cs ← Tạo mới chính sách
│   ├── VehicleTypeId [Required]
│   ├── PolicyName [Required, MaxLength 100]
│   ├── EffectiveStart [Required]
│   ├── EffectiveEnd (nullable)
│   └── PricingWindows: List<CreatePricingWindowRequest> [MinLength 1]
│
├── CreatePricingWindowRequest.cs ← Tạo mới khung giờ
│   ├── WindowName [Required]
│   ├── StartTime, EndTime [Required]
│   ├── BaseDurationMinutes [Range ≥ 1]
│   ├── BasePrice [Range ≥ 0]
│   ├── IncrementBlockMinutes [Range ≥ 1]
│   ├── IncrementPrice [Range ≥ 0]
│   ├── WindowCap (nullable)
│   └── GracePeriodMinutes [Range ≥ 0, Default 0]
│
├── UpdatePricingPolicyRequest.cs ← Cập nhật chính sách (partial)
│   ├── PolicyName (nullable)
│   ├── EffectiveStart, EffectiveEnd (nullable)
│   └── PricingPolicyStatus (nullable)
│
├── UpdatePricingWindowRequest.cs ← Cập nhật khung giờ (partial)
│   ├── WindowName, StartTime, EndTime (nullable)
│   ├── BaseDurationMinutes, BasePrice (nullable)
│   ├── IncrementBlockMinutes, IncrementPrice (nullable)
│   ├── WindowCap (nullable), RemoveWindowCap: bool
│   └── GracePeriodMinutes (nullable)
│
└── FeeCalculationResult.cs       ← Kết quả tính phí
    ├── TotalFee: decimal         (tổng phí cần thanh toán)
    └── Details: List<WindowFeeDetail>
        ├── PricingWindowId, WindowName
        ├── SegmentStart, SegmentEnd, SegmentMinutes
        ├── BaseCharge, IncrementBlocks, IncrementCharge
        ├── RawFee                (phí trước khi cap)
        └── CappedFee             (phí sau khi áp WindowCap)
```

### 2.3 Application Layer — Interfaces & Services

```
PBMS.Application/Pricing/
│
├── Interfaces/
│   ├── IPricingPolicyService.cs
│   │   ├── CreatePricingPolicyAsync(request)   → PricingPolicyDto
│   │   ├── GetPricingPolicyByIdAsync(id)        → PricingPolicyDto
│   │   ├── GetAllPricingPoliciesAsync(...)      → IEnumerable<PricingPolicyDto>
│   │   ├── UpdatePricingPolicyAsync(id, req)    → PricingPolicyDto
│   │   ├── AddPricingWindowAsync(policyId, req) → PricingWindowDto
│   │   ├── UpdatePricingWindowAsync(winId, req) → PricingWindowDto
│   │   └── DeletePricingWindowAsync(winId)      → void
│   │
│   └── IFeeCalculationService.cs
│       ├── CalculateFeeAsync(vehicleTypeId, checkIn, checkOut)         → FeeCalculationResult
│       └── CalculateFeeFromWindows(windows, checkIn, checkOut)         → FeeCalculationResult
│
└── Services/
    ├── PricingPolicyService.cs      ← Triển khai IPricingPolicyService
    └── FeeCalculationService.cs     ← Triển khai IFeeCalculationService
```

### 2.4 Application Layer — Repository Contract

```
PBMS.Application/Contracts/
└── IPricingPolicyRepository.cs     ← Kế thừa IRepository<PricingPolicy>
    ├── GetByIdWithWindowsAsync(id)                 → PricingPolicy?
    ├── GetAllWithWindowsAsync(vehicleTypeId, status) → IEnumerable<PricingPolicy>
    ├── GetActivePolicyAsync(vehicleTypeId, atTime)  → PricingPolicy?  ← dùng cho tính phí
    ├── GetWindowByIdAsync(windowId)                → PricingWindow?
    ├── AddWindowAsync(window)                      → void
    ├── UpdateWindow(window)                        → void
    ├── RemoveWindowAsync(window)                   → void
    └── CountWindowsByPolicyIdAsync(policyId)       → int
```

### 2.5 Infrastructure Layer

```
PBMS.Infrastructure/Repositories/
└── PricingPolicyRepository.cs    ← Kế thừa BaseRepository<PricingPolicy>
    └── Implements IPricingPolicyRepository
```

### 2.6 API Layer

```
PBMS.API/Controllers/
└── PricingPoliciesController.cs  ← Route: /api/pricing-policies
    ├── POST   /api/pricing-policies               → CreatePricingPolicy()
    ├── GET    /api/pricing-policies               → GetAllPricingPolicies()
    ├── GET    /api/pricing-policies/{id}          → GetPricingPolicyById()
    ├── PUT    /api/pricing-policies/{id}          → UpdatePricingPolicy()
    ├── POST   /api/pricing-policies/{id}/windows  → AddPricingWindow()
    ├── PUT    /api/pricing-policies/windows/{wId} → UpdatePricingWindow()
    └── DELETE /api/pricing-policies/windows/{wId} → DeletePricingWindow()
```

---

## 3. Luồng 1 — Tạo chính sách giá mới (Scenario 1)

```
Manager → POST /api/pricing-policies
          Body: {
            vehicleTypeId: 1,
            policyName: "Bảng giá xe máy vãng lai",
            effectiveStart: "2024-01-01",
            pricingWindows: [
              { windowName: "Khung giờ ngày", startTime: "06:00", endTime: "22:00",
                baseDurationMinutes: 60, basePrice: 5000,
                incrementBlockMinutes: 15, incrementPrice: 2000,
                windowCap: 50000, gracePeriodMinutes: 10 },
              { windowName: "Khung giờ đêm", startTime: "22:00", endTime: "06:00",
                baseDurationMinutes: 60, basePrice: 10000,
                incrementBlockMinutes: 30, incrementPrice: 5000 }
            ]
          }
```

**Luồng xử lý:**

```
PricingPoliciesController.CreatePricingPolicy()
    │
    ▼
PricingPolicyService.CreatePricingPolicyAsync()
    │
    ├── [1] Kiểm tra VehicleTypeId tồn tại
    │       _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId)
    │       → Nếu null: throw DomainException("VEHICLE_TYPE_NOT_FOUND")
    │
    ├── [2] Validate EffectiveEnd > EffectiveStart (nếu có)
    │       → Nếu sai: throw DomainException("INVALID_EFFECTIVE_DATE_RANGE")
    │
    ├── [3] Validate danh sách PricingWindows không rỗng
    │       → Nếu rỗng: throw DomainException("PRICING_WINDOWS_REQUIRED")
    │
    ├── [4] Validate từng PricingWindow
    │       ValidatePricingWindowParams()
    │       → BaseDuration > 0           | "INVALID_BASE_DURATION"
    │       → BasePrice >= 0             | "INVALID_BASE_PRICE"
    │       → IncrementBlock > 0         | "INVALID_INCREMENT_BLOCK"
    │       → IncrementPrice >= 0        | "INVALID_INCREMENT_PRICE"
    │       → WindowCap >= BasePrice     | "WINDOW_CAP_BELOW_BASE_PRICE"
    │
    ├── [5] Tạo PricingPolicy entity (status = "Active")
    │       + Tạo toàn bộ PricingWindow entities gắn vào policy
    │
    └── [6] _policyRepository.AddAsync(policy) → SaveChangesAsync()
            → Trả về PricingPolicyDto (201 Created)
```

---

## 4. Luồng 2 — Tính phí check-out (Scenario 2, 3, 4)

> Đây là **core logic** của tính năng, được gọi khi xe tiến hành check-out.

```
CheckoutService → IFeeCalculationService.CalculateFeeAsync(vehicleTypeId, checkIn, checkOut)
```

**Luồng xử lý:**

```
FeeCalculationService.CalculateFeeAsync()
    │
    ├── [1] GetActivePolicyAsync(vehicleTypeId, checkIn)
    │       Điều kiện Active policy:
    │         - PricingPolicyStatus == "Active"
    │         - VehicleTypeId trùng khớp
    │         - EffectiveStart <= checkIn.Date
    │         - EffectiveEnd IS NULL hoặc >= checkIn.Date
    │       → Nếu null: throw DomainException("PRICING_POLICY_NOT_FOUND")
    │
    └── [2] CalculateFeeFromWindows(policy.PricingWindows, checkIn, checkOut)
                │
                ▼
        SplitTimeIntoWindowSegments()  ← Phân tách thời gian
                │
                │  Duyệt từng ngày trong [checkIn, checkOut]:
                │    Với mỗi PricingWindow:
                │      - Tính windowStart, windowEnd trong ngày đó
                │      - Xử lý window qua đêm (EndTime < StartTime)
                │      - Tính intersection với [checkIn, checkOut]
                │      - Nếu đoạn > 0 phút → thêm vào danh sách
                │    Sắp xếp theo thời gian
                │
                ▼
        foreach (segment in segments):
            CalculateWindowSegmentFee(window, segStart, segEnd)
                │
                ├── totalMinutes = (segEnd - segStart).TotalMinutes
                │
                ├── [BR-FEE-004] Nếu totalMinutes <= BaseDurationMinutes:
                │       baseCharge = BasePrice
                │       incrementBlocks = 0
                │
                ├── [BR-FEE-005] Nếu totalMinutes > BaseDurationMinutes:
                │       baseCharge = BasePrice
                │       overMinutes = totalMinutes - BaseDurationMinutes
                │
                │       [BR-FEE-011] Nếu overMinutes <= GracePeriodMinutes:
                │           → KHÔNG tính block (Scenario 4 — Thời gian ân hạn)
                │
                │       [BR-FEE-012] Nếu overMinutes > GracePeriodMinutes:
                │           billableOver = overMinutes - GracePeriodMinutes
                │           incrementBlocks = Ceiling(billableOver / IncrementBlockMinutes)
                │           incrementCharge = incrementBlocks × IncrementPrice
                │
                ├── rawFee = baseCharge + incrementCharge
                │
                └── [BR-FEE-007] Áp WindowCap (Scenario 3):
                        Nếu WindowCap != null && rawFee > WindowCap:
                            cappedFee = WindowCap  ← Giới hạn tại mức trần
                        Else:
                            cappedFee = rawFee
                │
                ▼
        TotalFee = Σ(cappedFee của tất cả segments)
        → Trả về FeeCalculationResult { TotalFee, Details }
```

---

## 5. Ví dụ tính phí cụ thể

### Ví dụ 1 — Xe đỗ trong 1 khung giờ, vượt BaseDuration + GracePeriod (Scenario 2 & 4)

```
Cấu hình:
  Window ngày (06:00 - 22:00):
    BaseDuration = 60 phút | BasePrice = 5,000đ
    IncrementBlock = 15 phút | IncrementPrice = 2,000đ
    GracePeriod = 10 phút

Xe vào: 08:00 | Xe ra: 09:22 → 82 phút đỗ

Phân tách:
  Đoạn duy nhất: Window ngày [08:00 - 09:22] = 82 phút

Tính phí:
  BaseCharge  = 5,000đ (82p > 60p BaseDuration)
  OverMinutes = 82 - 60 = 22 phút
  22p > GracePeriod (10p) → CÓ tính block
  BillableOver = 22 - 10 = 12 phút
  IncrementBlocks = Ceiling(12/15) = 1 block
  IncrementCharge = 1 × 2,000 = 2,000đ
  RawFee = 5,000 + 2,000 = 7,000đ
  WindowCap = null → CappedFee = 7,000đ

TotalFee = 7,000đ ✅
```

### Ví dụ 2 — Xe đỗ qua 2 khung giờ (Scenario 2)

```
Cấu hình:
  Window ngày (06:00 - 22:00): Base 60p/5,000đ | Increment 15p/2,000đ
  Window đêm  (22:00 - 06:00): Base 60p/10,000đ | Increment 30p/5,000đ

Xe vào: 21:00 | Xe ra: 23:30 → 150 phút, vắt qua 22:00

Phân tách:
  Đoạn 1: Window ngày [21:00 - 22:00] = 60 phút
  Đoạn 2: Window đêm  [22:00 - 23:30] = 90 phút

Tính phí Đoạn 1 (Window ngày):
  60p = BaseDuration → BaseCharge = 5,000đ | Increment = 0
  CappedFee = 5,000đ

Tính phí Đoạn 2 (Window đêm):
  90p > 60p → BaseCharge = 10,000đ
  Over = 30p > GracePeriod (0p) → 1 block × 5,000 = 5,000đ
  CappedFee = 15,000đ

TotalFee = 5,000 + 15,000 = 20,000đ ✅
```

### Ví dụ 3 — WindowCap giới hạn phí (Scenario 3)

```
Cấu hình:
  Window ngày: BaseDuration 60p/5,000đ | Increment 15p/5,000đ | WindowCap = 50,000đ

Xe vào: 08:00 | Xe ra: 11:20 → 200 phút

Tính phí:
  BaseCharge  = 5,000đ
  Over = 200 - 60 = 140p → Ceiling(140/15) = 10 blocks → 50,000đ
  RawFee = 5,000 + 50,000 = 55,000đ
  55,000 > WindowCap (50,000) → CappedFee = 50,000đ ← Giới hạn lại!

TotalFee = 50,000đ ✅ (không phải 55,000đ)
```

---

## 6. Dependency Injection — Đăng ký services

```csharp
// PBMS.Application/DependencyInjection.cs
services.AddScoped<IPricingPolicyService, PricingPolicyService>();
services.AddScoped<IFeeCalculationService, FeeCalculationService>();

// PBMS.Infrastructure/DependencyInjection.cs
services.AddScoped<IPricingPolicyRepository, PricingPolicyRepository>();
```

---

## 7. Business Rules được triển khai

| Rule ID | Mô tả | Nơi áp dụng |
|---|---|---|
| BR-FEE-001 | Phí phụ thuộc loại xe | `GetActivePolicyAsync(vehicleTypeId)` |
| BR-FEE-002 | Tính từ check-in đến check-out | `CalculateFeeAsync(checkIn, checkOut)` |
| BR-FEE-003 | Mô hình Pricing Window | `SplitTimeIntoWindowSegments()` |
| BR-FEE-004 | BaseDuration → BasePrice | `CalculateWindowSegmentFee()` |
| BR-FEE-005 | Vượt BaseDuration → IncrementBlock | `CalculateWindowSegmentFee()` |
| BR-FEE-006 | Tách session theo từng window | `SplitTimeIntoWindowSegments()` |
| BR-FEE-007 | WindowCap chỉ áp per-window | `CalculateWindowSegmentFee()` |
| BR-FEE-008 | 24/7, không reset qua ngày | Duyệt từng ngày trong vòng lặp while |
| BR-FEE-011 | overMinutes ≤ Grace → không tính block | `if (overMinutes > GracePeriodMinutes)` |
| BR-FEE-012 | overMinutes > Grace → tính block mới | `incrementBlocks = Ceiling(billable / block)` |

---

## 8. Unit Tests — FeeCalculationServiceTests.cs

| Test method | Scenario | Kết quả kỳ vọng |
|---|---|---|
| `ShouldApplyBasePrice_WhenParkingWithinBaseDuration` | Đỗ 45p ≤ 60p BaseDuration | Fee = BasePrice = 5,000đ |
| `ShouldSplitByWindowBoundary_WhenParkingSpansTwoWindows` | Xe đỗ qua 2 window | Phân tách đúng, tổng = 20,000đ |
| `ShouldCapFeeAtWindowCap_WhenFeeExceedsCap` | Phí vượt WindowCap 50,000đ | Fee bị giới hạn = 50,000đ |
| `ShouldNotCapFee_WhenFeeIsUnderWindowCap` | Phí < WindowCap | Fee = RawFee, không bị cap |
| `ShouldIgnoreOvertime_WhenWithinGracePeriod` | Over 8p ≤ GracePeriod 10p | Không tính block, fee = BasePrice |
| `ShouldChargeIncrementBlock_WhenOvertimeExceedsGracePeriod` | Over 22p > GracePeriod 10p | Tính 1 block extra |
| `ShouldReturnZeroFee_WhenCheckOutEqualsOrBeforeCheckIn` | checkOut = checkIn | Fee = 0đ |
| `ShouldThrowDomainException_WhenNoPolicyFound` | Không có policy Active | Exception "PRICING_POLICY_NOT_FOUND" |

**Kết quả:** ✅ 41/41 tests passed

---

## 9. Error Codes

| Error Code | Nguyên nhân | HTTP Status |
|---|---|---|
| `VEHICLE_TYPE_NOT_FOUND` | VehicleTypeId không tồn tại | 404 |
| `INVALID_EFFECTIVE_DATE_RANGE` | EffectiveEnd ≤ EffectiveStart | 400 |
| `PRICING_WINDOWS_REQUIRED` | Danh sách window rỗng | 400 |
| `INVALID_BASE_DURATION` | BaseDurationMinutes ≤ 0 | 400 |
| `INVALID_BASE_PRICE` | BasePrice < 0 | 400 |
| `INVALID_INCREMENT_BLOCK` | IncrementBlockMinutes ≤ 0 | 400 |
| `INVALID_INCREMENT_PRICE` | IncrementPrice < 0 | 400 |
| `WINDOW_CAP_BELOW_BASE_PRICE` | WindowCap < BasePrice | 400 |
| `PRICING_POLICY_NOT_FOUND` | Không tìm thấy policy / không có Active policy | 404 |
| `PRICING_WINDOW_NOT_FOUND` | Không tìm thấy window theo ID | 404 |
| `CANNOT_DELETE_LAST_PRICING_WINDOW` | Xóa window cuối cùng của policy | 409 |
| `INVALID_PRICING_POLICY_STATUS` | Status không thuộc Active/Inactive/Expired | 400 |
