# 🚀 TÀI LIỆU HƯỚNG DẪN DEMO DỰ ÁN PBMS (DEMO GUIDE)
> **Hệ thống Quản lý Bãi đỗ Xe Thông minh (Parking Building Management System)**

---

## 📌 BẢNG TRA CỨU CẤU HÌNH HỆ THỐNG DỰ ÁN PBMS (SYSTEM CONFIG TRACEABILITY)
*Dành cho sinh viên trả lời khi Hội đồng / Giảng viên hỏi: "Thông số X này được cài đặt ở đâu trong code và DB?"*

| # | Tham số cấu hình | Giá trị mặc định | Bảng DB vật lý / File cấu hình | Entity / Cột / Key trong C# | API Endpoint xem/sửa | Màn hình Admin UI |
| :-: | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | **Thời gian đặt trước tối thiểu** | `15 phút` | `parking_system_config` / `appsettings.json` | `ParkingSystemConfig.Key = "MIN_BOOKING_ADVANCE_MINUTES"` / `BookingSettings.MinBookingMinutes` | `GET/PUT /api/v1/ParkingSystemConfig` | Cài đặt hệ thống ➔ Đặt chỗ |
| 2 | **Thời lượng đặt đỗ xe tối thiểu** | `4 giờ` | `appsettings.json` | `BookingSettings.MinBookingDurationHours` | Internal Config | Cài đặt hệ thống ➔ Đặt chỗ |
| 3 | **Hạn chờ thanh toán tiền cọc** | `15 phút` | `appsettings.json` | `BookingSettings.PaymentDeadlineMinutes` / `Booking.PaymentDeadline` | Internal Config | Cài đặt hệ thống ➔ Đặt chỗ |
| 4 | **Thời gian ân hạn nhận chỗ (Check-in Grace)** | `30 phút` | `appsettings.json` / `parking_system_config` | `BookingSettings.CheckinGracePeriodMinutes` / `Booking.CheckinGraceUntil` | `GET/PUT /api/v1/ParkingSystemConfig` | Cài đặt hệ thống ➔ Đặt chỗ |
| 5 | **Cho phép check-in sớm (Early Check-in)** | `30 phút` | `ParkingSessionService.cs` (Hardcoded logic) | `PlannedCheckinTime.AddMinutes(-30)` | `POST /api/v1/ParkingSessions/check-in` | Cài đặt hệ thống ➔ Cổng |
| 6 | **Thời gian ân hạn ra trễ (Late Checkout Grace)** | `15 phút` | `parking_system_config` | `ParkingSystemConfig.Key = "LATE_CHECKOUT_GRACE_PERIOD_MINUTES"` | `GET/PUT /api/v1/ParkingSystemConfig` | Cài đặt hệ thống ➔ Ra/Vào |
| 7 | **Khoảng đệm giữa 2 Booking (Buffer Time)** | `30 phút` | `parking_system_config` | `ParkingSystemConfig.Key = "BUFFER_TIME_MINUTES"` | `GET/PUT /api/v1/ParkingSystemConfig` | Cài đặt hệ thống ➔ Slot |
| 8 | **Giới hạn tỉ lệ đặt chỗ Zone (Booking Limit)** | `80%` | `zone` | `Zone.BookingLimitRate` | `GET/PUT /api/v1/Zones` | Sơ đồ & Khu vực ➔ Zone |
| 9 | **Bật tính phí phân đoạn (Segmented Pricing)** | `true` (Enabled) | `parking_system_config` | `ParkingSystemConfig.Key = "APPLY_SEGMENTED_PRICING"` | `GET/PUT /api/v1/ParkingSystemConfig` | Cài đặt hệ thống ➔ Tính phí |
| 10 | **Phí block cơ bản (Base Price & Duration)** | Máy: `60p / 5k`<br>Ô tô: `60p / 20k` | `pricing_window` / `base_pricing_rule_config` | `PricingWindow.BasePrice`, `BaseDurationMinutes` / `BasePricingRuleConfig` | `GET/PUT /api/v1/PricingPolicies` | Quản lý bảng giá |
| 11 | **Phí block phụ lũy tiến (Increment Price)** | Máy: `15p / 2k`<br>Ô tô: `15p / 5k` | `pricing_window` / `increment_pricing_rule_config` | `PricingWindow.IncrementPrice`, `IncrementBlockMinutes` / `IncrementPricingRuleConfig` | `GET/PUT /api/v1/PricingPolicies` | Quản lý bảng giá |
| 12 | **Phí phạt mất thẻ gửi xe (Lost Card)** | `100.000 VNĐ` | `penalty_config` | `PenaltyConfig.PenaltyFee` (`IncidentCode = "LOST_CARD"`) | `GET/PUT /api/v1/PenaltyConfigs` | Quản lý bảng phạt |

---

## 🔑 DỮ LIỆU SEED MẶC ĐỊNH TRONG DATABASE (`DbInitializer.cs`)

### 🔹 Tài Khoản Đăng Nhập
| Vai trò | Username | Password | Full Name | Mục đích Demo |
| :--- | :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `Password123` | System Admin | Quản lý toàn bộ cấu hình, bảng giá, sự cố |
| **Manager** | `manager` | `Password123` | John Doe (Manager) | Quản lý bãi đỗ, sơ đồ slot, báo cáo doanh thu |
| **Staff** | `staff` | `Password123` | Staff User | Nhân viên bảo vệ tại cổng: Check-in, Check-out, Sự cố |
| **Driver 1** | `driver` | `Password123` | Bob Johnson | Đặt chỗ, sở hữu xe Blacklist `51A-999.99` |
| **Driver 2** | `driver2` | `Password123` | Alice Smith | Đặt chỗ, có xe vào trễ `51G-888.88` & xe 80% ZC02 |
| **Driver 3** | `driver3` | `Password123` | Charlie Brown | Đặt chỗ, có xe đỗ qua đêm `51H-777.77` |

### 🔹 Danh Sách Phương Tiện & Trạng Thái Seed
| Biển số xe | Loại xe | Trạng thái Session / Booking | Slot / Zone | Mục đích Kịch bản Demo |
| :--- | :--- | :--- | :--- | :--- |
| **`51A-999.99`** | Ô tô | Bị `Blacklist` (`IsDeleted = false`) | - | **Demo 1**: Thử đăng nhập `driver` hoặc book/check-in ➔ Chặn lỗi `VEHICLE_BLACKLISTED`. |
| **`59T1-111.11`** | Xe máy | Session `COMPLETED` (Đỗ 3h, Tiền mặt) | `ZM01` | **Demo 2A**: Xe máy vãng lai đỗ xong checkout thành công. |
| **`59T1-222.22`** | Xe máy | Booking + Session `COMPLETED` (Online) | `ZM01` | **Demo 2B**: Xe máy booking đỗ xong checkout thành công. |
| **`30H-999.99`** | Ô tô | Session `COMPLETED` (Đỗ 3.5h, Online) | `ZC01-01` | **Demo 2C & 3**: Soi thuật toán Pricing Engine tính giá 40.000 VNĐ. |
| **`51G-67890`** | Ô tô | Booking + Session `COMPLETED` (Online) | `ZC01-02` | **Demo 2D**: Ô tô booking đỗ xong checkout thành công. |
| **`51H-333.33`** | Ô tô | Session `COMPLETED` (Đỗ 5h, Tiền mặt) | `ZC01` | **Demo 2E**: Ô tô vãng lai đỗ 5h checkout thành công. |
| **`51G-888.88`** | Ô tô | Booking `Confirmed` (Vào trễ 1 tiếng) | `ZC01-04` | **Demo 4**: Test xe đặt chỗ 1 tiếng trước nhưng chưa vào bãi (Check-in trễ). |
| **`51H-777.77`** | Ô tô | Session `ACTIVE` (Đỗ qua đêm 24h) | `ZC01-03` | **Demo 5**: Xe đang đỗ qua đêm, Slot `ZC01-03` hiển thị đỏ `Occupied`. |
| **`51K-000.01` ➔ `51K-000.20`** | Ô tô | 20 Booking `Confirmed` trùng lịch | `ZC02-01` ➔ `ZC02-20` | **Demo 6**: Fill đúng 80% (20/25) Zone `ZC02` Tầng 2 ➔ Slot thứ 21 báo lỗi `ZONE_BOOKING_LIMIT_EXCEEDED`. |

---

## ⏱️ THỨ TỰ THỰC HIỆN CÁC KỊCH BẢN DEMO (DEMO STEPS)

```mermaid
timeline
    title Sơ đồ Timeline Demo 20 Phút
    00:00 : Bước 0 - Reset DB & Khởi chạy Backend API local
    02:00 : Bước 1 - Demo Xe Blacklist (51A-999.99) bị hệ thống từ chối
    05:00 : Bước 2 - Demo Kiểm chứng Pricing Engine tính tiền (30H-999.99 đỗ 3.5h = 40k)
    09:00 : Bước 3 - Demo Xe Booking vào trễ 1h (51G-888.88) & Xe đỗ qua đêm 24h (51H-777.77)
    13:00 : Bước 4 - Demo Rate Limit 80% Capacity Tầng 2 Zone ZC02 (Slot 21 báo lỗi)
    17:00 : Bước 5 - Demo Live Action Check-in, Check-out & Khai báo Sự cố (Mất thẻ)
```

---

## 🛠️ BƯỚC 0: RESET DATABASE & CHẠY LOCAL BACKEND API

Mở PowerShell tại thư mục dự án `parking-system-api` và khởi chạy API:
```powershell
dotnet run --project src/PBMS.API --launch-profile Local
```
*Tác dụng*: Backend sẽ chạy EF Migration + `DbInitializer` để tạo sạch bãi đỗ với bộ dữ liệu seed gọn nhẹ chuẩn 100%.

---

## 🚫 BƯỚC 1: DEMO XE BLACKLIST KHÔNG ĐƯỢC ĐẶT CHỖ / CHECK-IN (Phút 02:00)

> **Mục đích**: Chứng minh hệ thống chặn xe nằm trong danh sách đen (`Blacklist`).

1. **Đăng nhập**: Mở Web App ➔ Đăng nhập bằng tài khoản `driver` / `Password123`.
2. **Thực hiện**: Chọn Đặt chỗ cho ô tô biển số **`51A-999.99`**.
3. **Kết quả**:
   - API trả về mã lỗi `400 Bad Request` với mã `VEHICLE_BLACKLISTED`.
   - Thông báo UI: *"Phương tiện '51A-999.99' đang nằm trong danh sách đen. Không thể đặt chỗ."*

---

## 💰 BƯỚC 2: DEMO KIỂM CHỨNG THUẬT TOÁN PRICING ENGINE (Phút 05:00)

> **Mục đích**: Kiểm chứng công thức tính tiền chuẩn của Pricing Engine cho xe vãng lai đỗ 3.5 tiếng.

1. **Thao tác**: Nhân viên quét xe / tìm hóa đơn lượt gửi của xe **`30H-999.99`** (đã đỗ từ 4h trước, ra 0.5h trước = 3.5h đỗ).
2. **Giải thích chi tiết cho Giảng viên**:
   - **Tổng thời gian gửi**: `3 giờ 30 phút` (210 phút).
   - **1 tiếng đầu (Base Duration)**: `20.000 VNĐ`.
   - **2.5 tiếng tiếp theo (Increment Duration)**: Mỗi 15 phút là 5.000 VNĐ. Tính tròn 10 block x 15 phút = `20.000 VNĐ` (đạt cap hoặc lũy tiến).
   - **Tổng hóa đơn**: **`40.000 VNĐ`**.

---

## ⏳ BƯỚC 3: DEMO XE BOOKING VÀO TRỄ 1H & XE ĐỖ QUA ĐÊM 24H (Phút 09:00)

### 📍 Phần 3A: Xe Booking Vào Trễ 1 Tiếng
1. **Kiểm tra**: Xem danh sách Booking của tài khoản `driver2` ➔ Có Booking xe **`51G-888.88`** đặt giờ vào từ 1 tiếng trước (`UtcNow - 1h`).
2. **Thao tác**: Nhân viên nhập biển **`51G-888.88`** tại cổng Check-in.
3. **Kết quả**: Hệ thống nhận diện Booking `CONFIRMED` quá hạn check-in grace period (30m) ➔ Tự động tính chuyển sang luồng xử lý tương ứng theo cấu hình.

### 📍 Phần 3B: Xe Đỗ Qua Đêm (24h)
1. **Kiểm tra**: Mở Sơ đồ Bãi đỗ (Tầng 1 - Zone `ZC01`).
2. **Kết quả**: Slot **`ZC01-03`** hiển thị trạng thái màu đỏ **`Occupied`** của xe **`51H-777.77`** (Check-in 24h trước, chưa Check-out).

---

## ⛔ BƯỚC 4: DEMO RATE LIMIT BOOKING 80% CAPACITY TẦNG 2 - ZC02 (Phút 13:00)

> **Mục đích**: Chứng minh quy tắc không cho đặt chỗ vượt quá **80% sức chứa** của một Zone.

1. **Bối cảnh trong Seed**: Zone `ZC02` Tầng 2 có sức chứa `Capacity = 25` slot. Đã có **20 xe (`51K-000.01` ➔ `51K-000.20`)** đặt chỗ trước ở khung giờ 2h - 6h tới ➔ Tải đặt chỗ đạt đúng **80%** (20/25 slot).
2. **Thao tác**: Đăng nhập `driver` ➔ Thực hiện Đặt chỗ tại **Zone `ZC02` Tầng 2** (chọn slot `ZC02-21`) trong cùng khung giờ.
3. **Kết quả**:
   - API chặn với lỗi `ZONE_BOOKING_LIMIT_EXCEEDED`.
   - Thông báo UI: *"Khu vực 'Car Zone F2' đã đạt giới hạn đặt trước tối đa (80% của sức chứa 25 = 20 vị trí). Tổng tải hiện tại: 20. Vui lòng chọn khu vực hoặc khung giờ khác."*

---

## 🚗 BƯỚC 5: DEMO LIVE ACTION CHECK-IN, CHECK-OUT & SỰ CỐ (Phút 17:00+)

1. **Check-in Xe Mới**: Bảo vệ chọn xe vãng lai `30A-123.45`, chọn thẻ `CARD004` ➔ Check-in ➔ Slot `ZC01-05` chuyển `Occupied`.
2. **Check-out Xe Mới**: Bảo vệ quét thẻ `CARD004` ➔ Hiển thị tiền đỗ ➔ Thanh toán ➔ Slot `ZC01-05` về `Available`.
3. **Khai Báo Sự Cố Mất Thẻ (`LOST_CARD`)**: Khai báo mất thẻ ➔ Thẻ bị `BLOCKED` ➔ Cộng tiền phạt `100.000 VNĐ` vào hóa đơn thanh toán.
