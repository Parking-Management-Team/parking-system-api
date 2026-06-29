> 🔍 Audited at commit: 6ba866a — 2026-06-24

# 📋 API Inventory

Tài liệu này liệt kê toàn bộ các API Endpoints hiện có trong dự án PBMS API, được quét trực tiếp từ mã nguồn thực tế ở tầng Web API Controllers.

> 💡 **Swagger / OpenAPI Contract Links (Khi ứng dụng đang chạy ở môi trường Local/Dev):**
> * **Swagger UI (Giao diện chạy thử trực quan)**: [http://localhost:5029/swagger](http://localhost:5029/swagger)
> * **OpenAPI Contract File (JSON)**: [http://localhost:5029/swagger/v1/swagger.json](http://localhost:5029/swagger/v1/swagger.json) (Dùng để auto-generate code HttpClient/TypeScript ở Frontend)

---

## 🔍 Danh Sách Chi Tiết API Endpoints

| Endpoint | Method | Module / Nghiệp vụ | File/Class | Có trong API_GUIDE.md? | Ghi chú |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `/api/auth/login` | `POST` | Xác thực | `AuthController` | ✅ Có | Đăng nhập bằng tài khoản và mật khẩu |
| `/api/auth/google` | `POST` | Xác thực | `AuthController` | ✅ Có | Đăng nhập qua Google OAuth 2.0 |
| `/api/accounts` | `GET` | Tài khoản | `AccountsController` | ✅ Có | Lấy danh sách tài khoản (Admin, Manager) |
| `/api/accounts/{id}` | `GET` | Tài khoản | `AccountsController` | ✅ Có | Lấy chi tiết tài khoản theo ID |
| `/api/accounts/{id}` | `PUT` | Tài khoản | `AccountsController` | ✅ Có | Cập nhật thông tin tài khoản |
| `/api/accounts/{id}` | `DELETE` | Tài khoản | `AccountsController` | ✅ Có | Khóa tài khoản (Admin) |
| `/api/accounts/{id}/deactivate` | `POST` | Tài khoản | `AccountsController` | ✅ Có | Tự vô hiệu hóa tài khoản của chính mình |
| `/api/accounts/change-password` | `POST` | Tài khoản | `AccountsController` | ✅ Có | Đổi mật khẩu tài khoản hiện tại |
| `/api/auditlogs` | `GET` | Tài khoản / Nhật ký | `AuditLogsController` | ❌ Chưa | Lấy danh sách nhật ký thao tác (Admin, Manager) |
| `/api/auditlogs/{id}` | `GET` | Tài khoản / Nhật ký | `AuditLogsController` | ❌ Chưa | Lấy chi tiết một bản ghi nhật ký |
| `/api/blacklist` | `POST` | Danh sách đen | `BlacklistController` | ✅ Có | Thêm xe/thẻ vi phạm vào danh sách đen |
| `/api/blacklist/{id}` | `DELETE` | Danh sách đen | `BlacklistController` | ✅ Có | Gỡ bỏ thực thể khỏi danh sách đen |
| `/api/blacklist/{id}` | `GET` | Danh sách đen | `BlacklistController` | ✅ Có | Xem chi tiết thông tin một bản ghi chặn |
| `/api/blacklist` | `GET` | Danh sách đen | `BlacklistController` | ✅ Có | Lấy danh sách đen có phân trang |
| `/api/blacklist/check-vehicle/{vehicleId}` | `GET` | Danh sách đen | `BlacklistController` | ✅ Có | Kiểm tra xem xe có bị chặn đỗ không |
| `/api/blacklist/check-card/{cardId}` | `GET` | Danh sách đen | `BlacklistController` | ✅ Có | Kiểm tra xem thẻ có bị chặn không |
| `/api/bookings` | `POST` | Đặt chỗ trước | `BookingsController` | ✅ [Tài liệu](booking-slot-selection-api.md#1-post-apibookings---create-booking) | Tạo đặt chỗ trước mới (chọn slot với xe hơi) |
| `/api/bookings` | `GET` | Đặt chỗ trước | `BookingsController` | ✅ [Tài liệu](booking-slot-selection-api.md#3-get-apibookings---list-bookings) | Lấy danh sách tất cả các đặt chỗ |
| `/api/bookings/{id}` | `GET` | Đặt chỗ trước | `BookingsController` | ✅ [Tài liệu](booking-slot-selection-api.md#2-get-apibookingsid---get-booking-detail) | Lấy chi tiết đặt chỗ theo ID |
| `/api/bookings/by-account/{accountId}` | `GET` | Đặt chỗ trước | `BookingsController` | ✅ Có | Lấy danh sách đặt chỗ của khách hàng |
| `/api/bookings/by-building/{buildingId}` | `GET` | Đặt chỗ trước | `BookingsController` | ✅ Có | Lấy danh sách đặt chỗ của một tòa nhà |
| `/api/bookings/{id}` | `PUT` | Đặt chỗ trước | `BookingsController` | ✅ Có | Cập nhật thời gian dự kiến check-in |
| `/api/bookings/{id}` | `DELETE` | Đặt chỗ trước | `BookingsController` | ✅ Có | Hủy bỏ đặt chỗ |
| `/api/bookings/cleanup` | `POST` | Đặt chỗ trước | `BookingsController` | ✅ Có | Dọn dẹp tự động các đặt chỗ quá hạn |
| `/api/buildings` | `POST` | Cấu trúc bãi xe | `BuildingsController` | ✅ Có | Tạo tòa nhà mới |
| `/api/buildings/{id}` | `GET` | Cấu trúc bãi xe | `BuildingsController` | ✅ Có | Lấy thông tin chi tiết tòa nhà |
| `/api/buildings` | `GET` | Cấu trúc bãi xe | `BuildingsController` | ✅ Có | Lấy danh sách toàn bộ tòa nhà |
| `/api/buildings/paged` | `GET` | Cấu trúc bãi xe | `BuildingsController` | ✅ Có | Lấy danh sách tòa nhà phân trang |
| `/api/buildings/{id}` | `PUT` | Cấu trúc bãi xe | `BuildingsController` | ✅ Có | Cập nhật thông tin tòa nhà |
| `/api/buildings/{id}` | `DELETE` | Cấu trúc bãi xe | `BuildingsController` | ✅ Có | Xóa thông tin tòa nhà |
| `/api/buildings/{id}/available-capacity` | `GET` | Cấu trúc bãi xe | `BuildingsController` | ❌ Chưa | Lấy sức chứa khả dụng theo thời gian đặt |
| `/api/cards` | `POST` | Quản lý thẻ | `CardController` | ✅ Có | Tạo mới thẻ vật lý RFID |
| `/api/cards` | `GET` | Quản lý thẻ | `CardController` | ✅ Có | Lấy danh sách tất cả thẻ |
| `/api/cards/{id}` | `GET` | Quản lý thẻ | `CardController` | ✅ Có | Xem chi tiết thẻ theo ID |
| `/api/cards/by-code/{cardCode}` | `GET` | Quản lý thẻ | `CardController` | ✅ Có | Tra cứu nhanh thẻ bằng mã thẻ (CardCode) |
| `/api/cards/{id}` | `PUT` | Quản lý thẻ | `CardController` | ✅ Có | Cập nhật thông tin RfidCode và CardType |
| `/api/cards/{id}` | `DELETE` | Quản lý thẻ | `CardController` | ✅ Có | Xóa thẻ khỏi hệ thống (nếu không bận) |
| `/api/cards/{id}/status` | `PUT` | Quản lý thẻ | `CardController` | ✅ Có | Cập nhật trạng thái thẻ (ví dụ báo mất) |
| `/api/floors` | `POST` | Cấu trúc bãi xe | `FloorsController` | ✅ Có | Tạo mới tầng trong tòa nhà |
| `/api/floors/{id}` | `GET` | Cấu trúc bãi xe | `FloorsController` | ✅ Có | Lấy chi tiết tầng theo ID |
| `/api/floors` | `GET` | Cấu trúc bãi xe | `FloorsController` | ✅ Có | Lấy danh sách tất cả các tầng |
| `/api/floors/building/{buildingId}` | `GET` | Cấu trúc bãi xe | `FloorsController` | ✅ Có | Lấy danh sách tầng của một tòa nhà |
| `/api/floors/paged` | `GET` | Cấu trúc bãi xe | `FloorsController` | ✅ Có | Lấy danh sách tầng có phân trang |
| `/api/floors/{id}` | `PUT` | Cấu trúc bãi xe | `FloorsController` | ✅ Có | Cập nhật thông tin tầng |
| `/api/floors/{id}` | `DELETE` | Cấu trúc bãi xe | `FloorsController` | ✅ Có | Xóa tầng khỏi hệ thống |
| `/api/incident` | `POST` | Quản lý sự cố | `IncidentController` | ✅ Có | Báo cáo sự cố mới |
| `/api/incident/{id}/status` | `PATCH` | Quản lý sự cố | `IncidentController` | ✅ Có | Cập nhật trạng thái xử lý sự cố |
| `/api/incident/{id}` | `GET` | Quản lý sự cố | `IncidentController` | ✅ Có | Xem chi tiết sự cố theo ID |
| `/api/incident` | `GET` | Quản lý sự cố | `IncidentController` | ✅ Có | Lấy danh sách sự cố có phân trang |
| `/api/incident/session/{sessionId}` | `GET` | Quản lý sự cố | `IncidentController` | ✅ Có | Lấy danh sách sự cố theo phiên gửi xe |
| `/api/incidenttype` | `GET` | Quản lý sự cố | `IncidentTypeController` | ✅ Có | Lấy tất cả phân loại loại sự cố |
| `/api/incidenttype/{id}` | `GET` | Quản lý sự cố | `IncidentTypeController` | ✅ Có | Xem chi tiết loại sự cố |
| `/api/monthly-subscriptions` | `POST` | Vé tháng | `MonthlySubscriptionsController` | ✅ Có | Đăng ký mua vé tháng mới |
| `/api/monthly-subscriptions/{id}` | `GET` | Vé tháng | `MonthlySubscriptionsController` | ✅ Có | Xem chi tiết thông tin vé tháng theo ID |
| `/api/monthly-subscriptions/{id}` | `DELETE` | Vé tháng | `MonthlySubscriptionsController` | ✅ Có | Hủy đăng ký vé tháng |
| `/api/monthly-subscriptions/cleanup` | `POST` | Vé tháng | `MonthlySubscriptionsController` | ✅ Có | Tự động dọn dẹp đăng ký quá hạn thanh toán |
| `/api/monthly-subscriptions/by-account/{accountId}` | `GET` | Vé tháng | `MonthlySubscriptionsController` | ✅ Có | Lấy danh sách đăng ký vé tháng của tài khoản |
| `/api/monthly-subscriptions/by-building/{buildingId}` | `GET` | Vé tháng | `MonthlySubscriptionsController` | ✅ Có | Lấy danh sách đăng ký vé tháng của tòa nhà |
| `/api/parking-sessions/check-in` | `POST` | Gửi xe thực tế | `ParkingSessionsController` | ✅ [Tài liệu](parking-sessions-api.md#1-post-apiparking-sessionscheck-in---check-in-vehicle) | Đăng ký xe vào bãi (Check-In) |
| `/api/parking-sessions` | `POST` | Gửi xe thực tế | `ParkingSessionsController` | ✅ Có | Tạo mới một phiên gửi xe |
| `/api/parking-sessions` | `GET` | Gửi xe thực tế | `ParkingSessionsController` | ✅ Có | Lấy danh sách tất cả các phiên gửi xe |
| `/api/parking-sessions/active` | `GET` | Gửi xe thực tế | `ParkingSessionsController` | ✅ Có | Lấy danh sách các xe đang đỗ trong bãi |
| `/api/parking-sessions/{id}` | `GET` | Gửi xe thực tế | `ParkingSessionsController` | ✅ Có | Xem chi tiết một phiên gửi xe theo ID |
| `/api/parking-sessions/{id}/slot` | `PATCH` | Gửi xe thực tế | `ParkingSessionsController` | ✅ Có | Gán vị trí đỗ (Slot) cho phiên gửi xe |
| `/api/parking-sessions/{id}/checkout/start` | `PATCH` | Gửi xe thực tế | `ParkingSessionsController` | ✅ [Tài liệu](parking-sessions-api.md#2-patch-apiparking-sessionsidcheckoutstart---start-checkout--evaluate-overtime) | Bắt đầu quy trình kiểm tra xe ra (Check-Out) |
| `/api/parking-sessions/{id}/complete` | `PATCH` | Gửi xe thực tế | `ParkingSessionsController` | ✅ [Tài liệu](parking-sessions-api.md#3-patch-apiparking-sessionsidcomplete---complete-checkout--resolve-incidents) | Hoàn tất phiên gửi xe (Xe rời bãi) |
| `/api/parking-sessions/{id}/checkout/rollback` | `PATCH` | Gửi xe thực tế | `ParkingSessionsController` | ✅ [Tài liệu](parking-sessions-api.md#4-patch-apiparking-sessionsidcheckoutrollback---rollback-checkout) | Hủy bỏ quy trình thanh toán/ra bãi |
| `/api/parkingslots` | `POST` | Cấu trúc bãi xe | `ParkingSlotsController` | ✅ Có | Tạo vị trí đỗ xe mới |
| `/api/parkingslots/{id}` | `GET` | Cấu trúc bãi xe | `ParkingSlotsController` | ✅ Có | Xem chi tiết vị trí đỗ xe |
| `/api/parkingslots` | `GET` | Cấu trúc bãi xe | `ParkingSlotsController` | ✅ Có | Lấy danh sách tất cả vị trí đỗ xe |
| `/api/parkingslots/zone/{zoneId}` | `GET` | Cấu trúc bãi xe | `ParkingSlotsController` | ✅ Có | Lấy danh sách vị trí đỗ theo khu vực (Zone), hỗ trợ lọc nâng cao theo trạng thái (statuses), loại xe (vehicleTypeIds), và tìm kiếm (search) |
| `/api/parkingslots/paged` | `GET` | Cấu trúc bãi xe | `ParkingSlotsController` | ✅ Có | Lấy danh sách vị trí đỗ phân trang |
| `/api/parkingslots/{id}` | `PUT` | Cấu trúc bãi xe | `ParkingSlotsController` | ✅ Có | Cập nhật thông tin vị trí đỗ xe |
| `/api/parkingslots/{id}` | `DELETE` | Cấu trúc bãi xe | `ParkingSlotsController` | ✅ Có | Xóa vị trí đỗ xe |
| `/api/payments` | `POST` | Thanh toán | `PaymentsController` | ✅ Có | Tạo mới yêu cầu thanh toán (Tiền mặt / Online) |
| `/api/payments/callback` | `GET` | Thanh toán | `PaymentsController` | ✅ Có | Điểm tiếp nhận IPN callback tự động từ VNPay |
| `/api/payments/vnpay-return` | `GET` | Thanh toán | `PaymentsController` | ❌ Chưa | Nhận redirect từ VNPay và chuyển hướng về FE |
| `/api/payments` | `GET` | Thanh toán | `PaymentsController` | ✅ Có | Lấy danh sách giao dịch thanh toán phân trang (Admin/Staff) |
| `/api/payments/by-session/{sessionId}` | `GET` | Thanh toán | `PaymentsController` | ✅ Có | Lấy danh sách giao dịch thanh toán theo sessionId |
| `/api/payments/by-account/{accountId}` | `GET` | Thanh toán | `PaymentsController` | ✅ Có | Lấy danh sách giao dịch thanh toán theo accountId |
| `/api/penalty-configs` | `GET` | Quản lý sự cố | `PenaltyConfigsController` | ❌ Chưa | Lấy danh sách cấu hình giá phạt sự cố |
| `/api/penalty-configs/active/{incidentTypeId}` | `GET` | Quản lý sự cố | `PenaltyConfigsController` | ❌ Chưa | Lấy cấu hình giá phạt đang hoạt động |
| `/api/penalty-configs` | `POST` | Quản lý sự cố | `PenaltyConfigsController` | ❌ Chưa | Tạo cấu hình giá phạt mới |
| `/api/penalty-configs/{id}/deactivate` | `PUT` | Quản lý sự cố | `PenaltyConfigsController` | ❌ Chưa | Vô hiệu hóa cấu hình giá phạt |
| `/api/penalty-configs/{id}` | `DELETE` | Quản lý sự cố | `PenaltyConfigsController` | ❌ Chưa | Xóa mềm cấu hình giá phạt |
| `/api/pricing-policies` | `POST` | Chính sách giá | `PricingPoliciesController` | ✅ Có | Tạo mới chính sách giá (trạng thái INACTIVE) |
| `/api/pricing-policies/{id}/activate` | `POST` | Chính sách giá | `PricingPoliciesController` | ✅ Có | Kích hoạt chính sách giá sau khi kiểm tra rule |
| `/api/pricing-policies` | `GET` | Chính sách giá | `PricingPoliciesController` | ✅ Có | Lấy danh sách tất cả các chính sách giá |
| `/api/pricing-policies/{id}` | `GET` | Chính sách giá | `PricingPoliciesController` | ✅ Có | Lấy thông tin chi tiết chính sách giá theo ID |
| `/api/pricing-policies/{id}` | `PUT` | Chính sách giá | `PricingPoliciesController` | ✅ Có | Cập nhật thông tin chính sách giá |
| `/api/pricing-policies/{id}/windows` | `POST` | Chính sách giá | `PricingPoliciesController` | ✅ Có | Thêm khung giờ tính giá mới vào chính sách |
| `/api/pricing-policies/windows/{windowId}` | `PUT` | Chính sách giá | `PricingPoliciesController` | ✅ Có | Cập nhật thông tin một khung giờ tính giá |
| `/api/pricing-policies/windows/{windowId}` | `DELETE` | Chính sách giá | `PricingPoliciesController` | ✅ Có | Xóa một khung giờ khỏi chính sách |
| `/api/pricing-policies/cleanup` | `POST` | Chính sách giá | `PricingPoliciesController` | ❌ Chưa | Dọn dẹp các chính sách giá Active quá hạn sang Expired |
| `/api/revenue` | `GET` | Báo cáo doanh thu | `RevenueController` | ✅ Có | Xem thống kê doanh thu theo bộ lọc, phân trang |
| `/api/revenue/{id}` | `GET` | Báo cáo doanh thu | `RevenueController` | ✅ Có | Xem chi tiết doanh thu và danh sách hóa đơn |
| `/api/subscription-price-configs` | `GET` | Vé tháng | `SubscriptionPriceConfigsController` | ❌ Chưa | Lấy danh sách cấu hình giá vé tháng |
| `/api/subscription-price-configs/active/{vehicleTypeId}` | `GET` | Vé tháng | `SubscriptionPriceConfigsController` | ❌ Chưa | Lấy cấu hình giá vé tháng đang hoạt động |
| `/api/subscription-price-configs` | `POST` | Vé tháng | `SubscriptionPriceConfigsController` | ❌ Chưa | Tạo cấu hình giá vé tháng mới |
| `/api/subscription-price-configs/{id}/deactivate` | `PUT` | Vé tháng | `SubscriptionPriceConfigsController` | ❌ Chưa | Vô hiệu hóa cấu hình giá vé tháng |
| `/api/subscription-price-configs/{id}` | `DELETE` | Vé tháng | `SubscriptionPriceConfigsController` | ❌ Chưa | Xóa mềm cấu hình giá vé tháng |
| `/api/vehicle-types` | `GET` | Phương tiện | `VehicleTypeController` | ✅ [Tài liệu](vehicle-types-api.md#1-get-apivehicle-types---list-all-vehicle-types) | Lấy danh sách tất cả các loại xe |
| `/api/vehicle-types/{id}` | `GET` | Phương tiện | `VehicleTypeController` | ✅ [Tài liệu](vehicle-types-api.md#2-get-apivehicle-typesid---vehicle-type-detail) | Xem chi tiết loại xe theo ID |
| `/api/vehicle-types` | `POST` | Phương tiện | `VehicleTypeController` | ✅ [Tài liệu](vehicle-types-api.md#3-post-apivehicle-types---create-vehicle-type) | Tạo mới loại xe |
| `/api/vehicle-types/{id}` | `PUT` | Phương tiện | `VehicleTypeController` | ✅ [Tài liệu](vehicle-types-api.md#4-put-apivehicle-typesid---update-vehicle-type-adminmanager) | Cập nhật loại xe |
| `/api/vehicle-types/{id}` | `DELETE` | Phương tiện | `VehicleTypeController` | ✅ Có | Xóa loại xe |
| `/api/vehicles` | `GET` | Phương tiện | `VehiclesController` | ✅ Có | Lấy danh sách xe (hoặc lọc theo AccountId) |
| `/api/vehicles/{id}` | `GET` | Phương tiện | `VehiclesController` | ✅ Có | Xem thông tin chi tiết xe theo ID |
| `/api/vehicles` | `POST` | Phương tiện | `VehiclesController` | ✅ Có | Đăng ký xe mới cho tài khoản khách hàng |
| `/api/vehicles/{id}` | `PUT` | Phương tiện | `VehiclesController` | ✅ Có | Cập nhật thông tin xe |
| `/api/vehicles/{id}` | `DELETE` | Phương tiện | `VehiclesController` | ✅ Có | Vô hiệu hóa/Lưu trữ xe (Archive) |
| `/api/zones` | `POST` | Cấu trúc bãi xe | `ZonesController` | ✅ Có | Tạo mới một khu vực (Zone) |
| `/api/zones/{id}` | `GET` | Cấu trúc bãi xe | `ZonesController` | ✅ Có | Lấy chi tiết thông tin Zone |
| `/api/zones` | `GET` | Cấu trúc bãi xe | `ZonesController` | ✅ Có | Lấy toàn bộ danh sách các Zone |
| `/api/zones/floor/{floorId}` | `GET` | Cấu trúc bãi xe | `ZonesController` | ✅ Có | Lấy danh sách Zone thuộc một tầng cụ thể |
| `/api/zones/paged` | `GET` | Cấu trúc bãi xe | `ZonesController` | ✅ Có | Lấy danh sách Zone có phân trang |
| `/api/zones/{id}` | `PUT` | Cấu trúc bãi xe | `ZonesController` | ✅ Có | Cập nhật thông tin Zone |
| `/api/zones/{id}` | `DELETE` | Cấu trúc bãi xe | `ZonesController` | ✅ Có | Xóa Zone khỏi hệ thống |
