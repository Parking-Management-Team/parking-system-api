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
| 8 | **Bật tính phí phân đoạn (Segmented Pricing)** | `true` (Enabled) | `parking_system_config` | `ParkingSystemConfig.Key = "APPLY_SEGMENTED_PRICING"` | `GET/PUT /api/v1/ParkingSystemConfig` | Cài đặt hệ thống ➔ Tính phí |
| 9 | **Thời gian ân hạn miễn phí đỗ lố (Fee Grace)** | `0 - 15 phút` | `pricing_window` / `grace_period_rule_config` | `PricingWindow.GracePeriodMinutes` / `GracePeriodRuleConfig.GracePeriodMinutes` | `GET/PUT /api/v1/PricingPolicies` | Quản lý bảng giá ➔ Pricing Window |
| 10 | **Phí block cơ bản (Base Price & Duration)** | Máy: `60p / 5k`<br>Ô tô: `60p / 20k` | `pricing_window` / `base_pricing_rule_config` | `PricingWindow.BasePrice`, `BaseDurationMinutes` / `BasePricingRuleConfig` | `GET/PUT /api/v1/PricingPolicies` | Quản lý bảng giá |
| 11 | **Phí block phụ lũy tiến (Increment Price)** | Máy: `15p / 2k`<br>Ô tô: `15p / 5k` | `pricing_window` / `increment_pricing_rule_config` | `PricingWindow.IncrementPrice`, `IncrementBlockMinutes` / `IncrementPricingRuleConfig` | `GET/PUT /api/v1/PricingPolicies` | Quản lý bảng giá |
| 12 | **Ngưỡng tính phí block lẻ (Threshold %)** | `50%` | `increment_pricing_rule_config` | `IncrementPricingRuleConfig.ThresholdPercentage` | `GET/PUT /api/v1/PricingPolicies` | Quản lý quy tắc bảng giá |
| 13 | **Mức giá trần tối đa/ngày (Daily Cap)** | Máy: `50k/ngày`<br>Ô tô: `150k/ngày` | `daily_cap_rule_config` | `DailyCapRuleConfig.MaximumDailyAmount` | `GET/PUT /api/v1/PricingPolicies` | Quản lý bảng giá ➔ Daily Cap |
| 14 | **Độ ưu tiên bảng giá (Priority)** | Mặc định `0`, Lễ `1` | `pricing_policy` | `PricingPolicy.Priority` | `GET/PUT /api/v1/PricingPolicies` | Quản lý bảng giá ➔ Priority |
| 15 | **Phí phạt mất thẻ gửi xe (Lost Card)** | `100.000 VNĐ` | `penalty_config` | `PenaltyConfig.PenaltyFee` (`IncidentCode = "LOST_CARD"`) | `GET/PUT /api/v1/PenaltyConfigs` | Quản lý bảng phạt |
| 16 | **Phí phạt đỗ xe quá giờ (Late Checkout)** | `50.000 VNĐ` | `penalty_config` | `PenaltyConfig.PenaltyFee` (`IncidentCode = "LATE_CHECKOUT"`) | `GET/PUT /api/v1/PenaltyConfigs` | Quản lý bảng phạt |
| 17 | **Mệnh giá làm tròn tiền mặt (Cash Rounding)** | Unit: `500đ`<br>Threshold: `250đ` | `appsettings.json` | `PaymentSettings.CashRoundingUnit`, `RoundingThreshold` | Internal Config | Cài đặt hệ thống ➔ Thanh toán |
| 18 | **Thời hạn hủy Booking được hoàn cọc** | `60 phút` trước giờ check-in | `BookingService.cs` (Hardcoded logic) | `booking.PlannedCheckinTime - DateTime.UtcNow >= 60m` | `DELETE /api/v1/Bookings/{id}` | Quản lý Đặt chỗ |
| 19 | **Thời gian sống của JWT Access Token** | `1440 phút` (24h) | `appsettings.json` | `Jwt.ExpiryInMinutes` | `POST /api/v1/Auth/login` | Cài đặt hệ thống ➔ Auth |

---

## ⏱️ THỨ TỰ THỰC HIỆN CÓ TÍNH TOÁN THỜI GIAN (CALCULATED TIMELINE SEQUENCE)

Lý do cần tính toán mốc thời gian: Theo quy định hệ thống, khách không thể đặt giờ Booking trùng với giờ hiện tại (`T_0`) mà phải đặt trước ít nhất 15 phút (`T_0 + 15m`). Do đó, ta sẽ cho khách **bấm Đặt chỗ ngay ở phút đầu tiên của buổi demo**, sau đó tận dụng 15 phút chờ để demo các luồng xe thường, tính tiền và sự cố!

```mermaid
timeline
    title Sơ đồ Timeline Demo 20 Phút
    00:00 : Bước 0 - Dọn dẹp dữ liệu cũ trên Supabase & Chạy Backend (DbInitializer)
          : Bước 1 - Khách bấm Đặt chỗ Booking (Giờ bắt đầu: T + 15 phút)
    03:00 : Bước 2 - Demo Xe Thường Live Check-in & Check-out ngay
    07:00 : Bước 3 - Demo Xe Seed (3.5h) Check-out để soi Pricing Engine
    12:00 : Bước 4 - Demo Luồng Lỗi: Mất thẻ, Sai loại xe, Blacklist
    16:00 : Bước 5 - Khóa đuôi: Xe Booking đến đỗ (Thời gian thực vừa khớp T + 15m)
```

---

## 🛠️ BƯỚC 0: DỌN DẸP DỮ LIỆU CŨ TRÊN SUPABASE & SEED DATABASE

> ⚠️ **LƯU Ý QUAN TRỌNG VỀ SUPABASE**:
> Hàm `DbInitializer.cs` mặc định **KHÔNG tự động DROP/Delete** dữ liệu cũ nếu DB Supabase đã có dữ liệu từ trước (nó chỉ chèn bổ sung bản ghi chưa có).
> Do đó, để đảm bảo bãi xe sạch 100% khi bước vào buổi Demo, bạn hãy thực hiện theo 1 trong 2 cách sau:

### 🔹 Cách 1: Dọn dẹp dữ liệu giao dịch cũ (Khuyên dùng - Nhanh nhất)
Mở **Supabase SQL Editor** và chạy câu lệnh TRUNCATE các bảng dữ liệu phát sinh (giữ nguyên bảng cấu hình `building`, `floor`, `account`):
```sql
TRUNCATE TABLE parking_session, booking, payment, invoice, incident CASCADE;
```
*Tác dụng*: Xóa toàn bộ các lượt gửi xe rác trước đó, đưa các vị trí đỗ xe về trạng thái sẵn sàng.

### 🔹 Cách 2: Reset hoàn toàn Database Supabase
Nếu muốn reset toàn bộ về ban đầu:
1. Vào Supabase SQL Editor chạy: `DROP SCHEMA public CASCADE; CREATE SCHEMA public;`
2. Khởi chạy lại Backend:
   ```powershell
   dotnet run --project src/PBMS.API --launch-profile Local
   ```
3. Backend sẽ tự động chạy EF Migration + `DbInitializer` để nạp mới 100% dữ liệu chuẩn.

---

## 📅 BƯỚC 1: KHÁCH HÀNG THỰC HIỆN ĐẶT CHỖ TRƯỚC (Phút 00:00 - 03:00)

> **Mục đích**: Khởi tạo Booking ngay đầu buổi để đến Phút 15:00 thời gian thực vừa kịp khớp giờ vào đỗ.

1. **Khách hàng mở Web App**:
   - Chọn Tầng B1 ➔ Chọn Slot `ZC01-05`.
   - Chọn thời gian gửi:
     - Giờ bắt đầu (`StartTime`): **Thời điểm hiện tại + 15 phút** (VD: Hiện tại 14:00 ➔ Chọn 14:15).
     - Giờ kết thúc (`EndTime`): **Thời điểm hiện tại + 4 tiếng 15 phút** (VD: 18:30).
2. **Thanh toán cọc & Tạo Booking**:
   - Khách bấm "Thanh toán cọc" ➔ Hệ thống gọi `POST /api/v1/Bookings`.
3. **Kết quả quan sát trên UI**:
   - Slot `ZC01-05` được chuyển sang trạng thái `Reserved`.
   - *Ghi chú cho Giảng viên*: *"Booking đã được ghi nhận. Bây giờ hệ thống sẽ giữ slot này. Trong lúc chờ đến 14:15 để xe booking vào, em xin phép demo luồng xe vãng lai và tính tiền."*

---

## 🚗 BƯỚC 2: DEMO LUỒNG XE THƯỜNG / VÃNG LAI LIVE (Phút 03:00 - 07:00)

### 📍 Phần 2A: Check-in Xe Mới (Live Action)
1. **Thao tác**:
   - Bảo vệ quét biển số `30A-123.45`, chọn loại `Ô tô`, chọn thẻ `CARD001`.
   - Gọi `POST /api/v1/ParkingSessions/check-in`.
2. **Kết quả UI**: Slot `ZC01-01` tự động chuyển sang trạng thái `Occupied`.

### 📍 Phần 2B: Check-out Xe Mới (Live Action)
1. **Thao tác**: Bảo vệ quét thẻ `CARD001` ra cổng ➔ Gọi `POST /api/v1/ParkingSessions/checkout/start`.
2. **Kết quả UI**: Slot `ZC01-01` chuyển về trạng thái `Available`.

---

## 💰 BƯỚC 3: KIỂM CHỨNG THUẬT TOÁN PRICING ENGINE (Phút 07:00 - 11:00)

> **Mục đích**: Check-out xe `30H-999.99` đã đỗ 3.5 tiếng từ bước Seed để chứng minh tính đúng đắn của bộ tính tiền.

1. **Thao tác**:
   - Bảo vệ quét thẻ `CARD002` (Xe `30H-999.99`).
   - Gọi `POST /api/v1/ParkingSessions/checkout/start`.
2. **Giải thích chi tiết cho Giảng viên trên Màn hình Hóa đơn**:
   - **Tổng thời gian đỗ**: `3 giờ 30 phút` (Tài khoản xem ở `check_in_time`).
   - **Bảng giá áp dụng**: `PricingPolicy (Bảng giá ô tô ban ngày)` (Cài đặt tại `pricing_policy`).
   - **Công thức tính toán của Pricing Engine**:
     - *2 tiếng đầu*: `20.000 VNĐ` (Cài đặt tại `pricing_rule.first_block_price`).
     - *1.5 tiếng tiếp theo*: Tính tròn 2 block 1h x 10.000đ = `20.000 VNĐ` (Cài đặt tại `increment_pricing_rule_config`).
     - **Tổng tiền**: **`40.000 VNĐ`**.
3. **Hoàn tất**: Bấm Thanh toán ➔ Slot `ZC01-02` giải phóng về `Available`.

---

## ⚠️ BƯỚC 4: DEMO LUỒNG LỖI & XỬ LÝ SỰ CỐ INCIDENTS (Phút 11:00 - 15:00)

### 📍 Sự cố 4.1: Mất Thẻ Gửi Xe (`LOST_CARD`)
1. **Thao tác**: Khách báo mất thẻ ➔ Bảo vệ mở màn hình *Khai báo Sự cố (Incident)* ➔ Chọn loại `LOST_CARD`.
2. **Giải thích cho Giảng viên**:
   - Phí phạt mất thẻ `100.000 VNĐ` được lấy từ bảng `penalty_config` (Key: `LOST_CARD`).
   - Tổng hóa đơn = `Tiền đỗ xe + 100.000 VNĐ phạt`.
   - Thẻ cũ bị đánh dấu `BLOCKED` trong bảng `card`.

### 📍 Sự cố 4.2: Biển Số Sai Loại Xe (`VEHICLE_TYPE_MISMATCH`)
1. **Thao tác**: Nhập biển ô tô `30A-888.88` nhưng chọn thẻ Xe máy.
2. **Kết quả**: API chặn với mã lỗi `400 Bad Request (VEHICLE_TYPE_MISMATCH)` do regex kiểm tra biển số không khớp.

### 📍 Sự cố 4.3: Xe Blacklist (`BLACKLISTED_VEHICLE`)
1. **Thao tác**: Nhập biển số nằm trong danh sách bùng tiền.
2. **Kết quả**: Hệ thống cảnh báo đỏ và từ chối tạo Session.

---

## 🏁 BƯỚC 5: KHÓA ĐUÔI - XE BOOKING ĐẾN ĐỖ ĐÚNG GIỜ (Phút 15:00+)

> **Lúc này thời gian thực đã vừa tròn 15 phút kể từ Bước 1!**

1. **Xe Booking đến bãi (Lúc 14:15 - Đúng giờ đặt)**:
   - Bảo vệ quét biển số `29A-888.88` hoặc Mã Booking ở Bước 1.
   - Hệ thống tự động nhận diện có Booking `CONFIRMED` đang hiệu lực.
2. **Kết quả quan sát trên UI**:
   - Trạng thái Booking chuyển sang `CHECKED_IN`.
   - Slot `ZC01-05` tự động chuyển sang `Occupied`.
3. **Check-out xe Booking**:
   - Quét xe ra ➔ Hệ thống tự động khấu trừ tiền đặt cọc đã trả ở Bước 1 ➔ Slot `ZC01-05` giải phóng về `Available`.
