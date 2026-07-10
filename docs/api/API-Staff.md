# Yêu cầu Backend bổ sung cho luồng Staff sau khi đối chiếu API + PBMS SRS

Ngày cập nhật: 2026-07-10

Tài liệu này được lập sau khi đối chiếu:

- `parking-system-api/docs/API_GUIDE.md`
- `parking-system-api/docs/API_INVENTORY.md`
- `parking-system-api/docs/API_READINESS_AUDIT.md`
- `PBMS-SRS-main/PBMS-SRS-main/PBMS_SRS_Document.md`
- `PBMS-SRS-main/PBMS-SRS-main/PBMS_Feature_Actor_Based.md`

## Kết luận đối chiếu nhanh

Các yêu cầu Backend dưới đây nhìn chung **không trái logic SRS**, nhưng cần chỉnh một điểm quan trọng:

- Không nên thiết kế Payment có nguồn chính là `incident_id` riêng lẻ nếu chưa đổi SRS/schema.
- Theo SRS, `payment` chỉ có đúng một nguồn nghiệp vụ chính: `session_id`, `booking_id` hoặc `monthly_subscription_id`.
- Vì vậy phí incident/lost card/wrong zone nên được cộng vào **Checkout Payment của Parking Session**, kèm breakdown chi tiết, thay vì tạo payment nguồn incident độc lập.

## Danh sách API Backend còn thiếu / nên bổ sung

| Ưu tiên | API / thay đổi đề xuất | Lý do nghiệp vụ |
|---|---|---|
| Cao | `POST /api/parking-sessions/{sessionId}/lost-card` | Tạo incident mất thẻ + đổi card sang `Lost` trong một transaction, tránh FE gọi rời 2 API. |
| Cao | `POST /api/parking-sessions/{sessionId}/lost-card/rollback` hoặc `POST /api/incidents/{incidentId}/rollback` | Hủy mất thẻ an toàn: rollback incident + khôi phục card status đúng theo session. |
| Cao | Mở rộng `POST /api/payments` cho session checkout để nhận `incidentIds` hoặc tự trả fee breakdown | Staff cần hiển thị phí gửi xe, từng phí incident, tổng incident fee, tổng cần trả. |
| Cao | Payment calculation nên cộng incident `OPEN` và cân nhắc `PROCESSING` theo rule rõ ràng | SRS cho phép Staff xử lý incident trước checkout; nếu status đổi `PROCESSING` mà BE chỉ cộng `OPEN` thì phí có thể lệch. |
| Cao | `POST /api/parking-sessions/{sessionId}/unpaid-checkout` | Khách không trả tiền phải tạo unpaid incident + blacklist trong một transaction. |
| Trung bình | `GET /api/payments/session/{sessionId}` hoặc thêm payment summary vào `GET /api/parking-sessions` | Staff check-out history cần tổng tiền/phương thức/trạng thái thanh toán thật từ BE. |
| Trung bình | `POST /api/shift-reports`, `GET /api/shift-reports` | Staff gửi báo cáo ca đúng luồng SRS Staff Shift Report. |
| Trung bình | API notification cho Manager theo schema SRS hiện tại | Staff report / unpaid issue cần đẩy thông báo cho Manager. |
| Thấp | `GET /api/Blacklist/check-license-plate/{licensePlate}` | Check-in có thể chỉ có biển số, chưa có `vehicleId`, nên cần check blacklist theo plate. |

## 1. Lost card phải là thao tác nguyên tử

SRS liên quan:

- BR-052: Staff chuyển vé/mã gửi xe thành `LOST` thì hệ thống áp dụng lost card penalty.
- BR-053: Xe mất thẻ chỉ được ra sau khi thanh toán phí gửi xe hiện tại và lost card penalty.

Hiện FE đang phải gọi:

1. `POST /api/Incident`
2. `PUT /api/cards/{id}/status` với `Lost`

Đề xuất:

```http
POST /api/parking-sessions/{sessionId}/lost-card
```

Body:

```json
{
  "incidentTypeId": 1,
  "staffId": 2,
  "description": "Khách báo mất thẻ tại cổng ra"
}
```

Backend nên xử lý trong transaction:

- Validate session đang active.
- Validate session có card.
- Tạo incident `LOST_CARD`.
- Áp penalty theo `IncidentType/PenaltyConfig`.
- Đổi card status sang `Lost`.
- Trả về incident + card/session mới nhất.

## 2. Rollback lost card phải do Backend quyết định card status

Hiện FE có thể xóa incident rồi set card về `Active`. Cách này chạy demo được nhưng chưa an toàn, vì BE mới biết card nên về:

- `Active`: session vẫn active.
- `Available`: session đã completed.
- `Assigned`: monthly card.

Đề xuất:

```http
POST /api/parking-sessions/{sessionId}/lost-card/rollback
```

Hoặc:

```http
POST /api/incidents/{incidentId}/rollback
```

## 3. Payment breakdown đúng SRS

SRS yêu cầu:

- Staff check-out phải tính phí, thu tiền, hiển thị tổng phí cần thanh toán.
- Nếu có phí phạt/phụ phí, hệ thống cộng vào tổng phí trước thanh toán.
- Payment phát sinh từ session phải lưu policy/detail đủ để audit khi bảng giá thay đổi.

Đề xuất mở rộng response `POST /api/payments`:

```json
{
  "paymentId": 123,
  "sessionId": 10,
  "amount": 4790000,
  "baseParkingFee": 4760000,
  "incidentFeeTotal": 30000,
  "paymentMethod": "CASH",
  "paymentStatus": "PAID",
  "items": [
    {
      "type": "PARKING_FEE",
      "name": "Phí gửi xe",
      "amount": 4760000
    },
    {
      "type": "INCIDENT_PENALTY",
      "incidentId": 9,
      "name": "Wrong slot",
      "amount": 30000
    }
  ]
}
```

Không đề xuất payment nguồn `incident_id` riêng nếu chưa sửa SRS/schema.

## 4. Incident status và payment calculation

API readiness audit hiện ghi BE đã cộng incident penalty đang mở (`IncidentStatus.Open`) vào checkout payment.

Vấn đề:

- FE muốn Staff chọn incident và incident chuyển sang `PROCESSING`.
- Nếu BE chỉ cộng `OPEN`, payment có thể thiếu phí.

Đề xuất một trong hai cách:

### Cách A

Payment cộng incident status:

- `OPEN`
- `PROCESSING`

### Cách B

Thêm field nghiệp vụ:

- `includeInCheckoutPayment`
- hoặc `isChargeable`

Khi đó status xử lý và trạng thái tính tiền không bị phụ thuộc nhau.

## 5. Unpaid checkout nên có API riêng

SRS:

- Xe chưa thanh toán không được hoàn tất check-out, trừ khi Manager xử lý đặc biệt.
- Incident có thể dẫn đến blacklist.

Hiện FE gọi rời:

1. `POST /api/Incident`
2. `POST /api/Blacklist`

Đề xuất:

```http
POST /api/parking-sessions/{sessionId}/unpaid-checkout
```

Backend nên xử lý:

- Tạo incident `UNPAID_VEHICLE`.
- Tạo blacklist cho vehicle/card liên quan.
- Không complete session nếu chưa có quyền Manager xử lý đặc biệt.
- Trả trạng thái để FE hiển thị cho Staff/Manager.

## 6. Check blacklist theo biển số

API hiện có:

- `GET /api/Blacklist/check-vehicle/{vehicleId}`
- `GET /api/Blacklist/check-card/{cardId}`

Thiếu:

```http
GET /api/Blacklist/check-license-plate/{licensePlate}
```

Lý do:

- Staff Check-in nhập biển số trước, có thể chưa có `vehicleId`.
- SRS yêu cầu validate blacklist status khi check-in.

## 7. Shift report và Manager notification

SRS Staff Portal có Shift Report:

- View daily activity.
- View payment summary.
- View incident summary.
- View check-in/check-out count.

Nếu muốn Staff gửi báo cáo để Manager thấy thông báo:

```http
POST /api/shift-reports
GET /api/shift-reports
```

Notification nên bám theo schema SRS hiện tại. Nếu schema notification chưa có read time/type thì không ép thêm field ngoài SRS, chỉ cần trạng thái đủ để Manager thấy/chưa thấy thông báo.

