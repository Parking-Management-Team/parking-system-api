# Hướng dẫn Kiểm tra và Kiểm thử Tính năng Quản lý Loại Xe (Vehicle Type)

Tài liệu này hướng dẫn bạn cách kiểm tra các tính năng mà AI Agent đã triển khai cho hệ thống PBMS.

## 1. Chuẩn bị Môi trường

### Bước 1: Cập nhật Cơ sở dữ liệu
Mở terminal tại thư mục `parking-system-api` và chạy lệnh sau để áp dụng migration:

```bash
dotnet ef database update --project src/PBMS.Infrastructure --startup-project src/PBMS.API
```

### Bước 2: Chạy Ứng dụng
Chạy API project:

```bash
dotnet run --project src/PBMS.API
```

API sẽ mặc định chạy tại: `https://localhost:5001` hoặc `http://localhost:5000` (Kiểm tra terminal output để biết port chính xác).

---

## 2. Các Kịch bản Kiểm thử (Test Scenarios)

Bạn có thể sử dụng **Swagger UI** tại địa chỉ: `https://localhost:5029/swagger/index.html` (hoặc port tương ứng) để test trực quan.

### Kịch bản 1: Kiểm tra Dữ liệu mặc định (Seed Data)
1. **Endpoint:** `GET /api/vehicletype`
2. **Mong đợi:** Trả về danh sách có ít nhất 2 loại xe:
   - ID 1: "Xe Máy"
   - ID 2: "Ô Tô"
3. **Mã trạng thái:** 200 OK

### Kịch bản 2: Tạo Loại xe mới
1. **Endpoint:** `POST /api/vehicletype`
2. **Body:**
   ```json
   { "name": "Xe Đạp" }
   ```
3. **Mong đợi:** Tạo thành công, trả về ID mới và `isActive: true`.
4. **Mã trạng thái:** 201 Created

### Kịch bản 3: Kiểm tra Ràng buộc Trùng tên
1. **Endpoint:** `POST /api/vehicletype`
2. **Body:**
   ```json
   { "name": "Xe Máy" }
   ```
3. **Mong đợi:** Không cho phép tạo, trả về lỗi tiếng Việt.
   - `success: false`
   - `errorCode: "NAME_EXISTS"`
   - `message: "Loại xe 'Xe Máy' đã tồn tại trong hệ thống."`
4. **Mã trạng thái:** 400 Bad Request

### Kịch bản 4: Cập nhật Loại xe
1. **Endpoint:** `PUT /api/vehicletype/{id}` (ví dụ ID 1)
2. **Body:**
   ```json
   { "name": "Xe Máy Phân Khối Lớn", "isActive": false }
   ```
3. **Mong đợi:** Cập nhật thành công tên và trạng thái.
4. **Mã trạng thái:** 200 OK

### Kịch bản 5: Xóa Loại xe
1. **Endpoint:** `DELETE /api/vehicletype/{id}`
2. **Trường hợp 1 (ID không tồn tại):** Mong đợi lỗi `NOT_FOUND` (404).
3. **Trường hợp 2 (ID đang được sử dụng):** Nếu loại xe đã có dữ liệu trong `ParkingSessions` hoặc `Bookings`, hệ thống sẽ trả về lỗi `IN_USE_SESSIONS` hoặc `IN_USE_BOOKINGS`.

---

## 3. Lưu ý về Quyền truy cập (Authorization)

Các endpoint hiện tại đang được đánh dấu bằng thuộc tính `[Authorize]`. 

**Nếu bạn gặp lỗi 401 Unauthorized khi test:**
1. Do hệ thống Authentication chưa được cấu hình hoàn chỉnh trong `Program.cs`, bạn có thể tạm thời comment dòng `[Authorize]` trong file `VehicleTypeController.cs` để test nhanh các logic nghiệp vụ.
2. Hoặc cấu hình JWT/Identity nếu dự án đã có module này.

## 4. Cấu trúc Code cần kiểm tra

- **Domain:** `src/PBMS.Domain/Entities/VehicleType.cs`
- **Logic:** `src/PBMS.Application/Vehicle/Services/VehicleTypeService.cs`
- **Repository:** `src/PBMS.Infrastructure/Repositories/VehicleTypeRepository.cs`
- **Controller:** `src/PBMS.API/Controllers/VehicleTypeController.cs`

---
*Tài liệu được tạo bởi Gemini CLI để hỗ trợ kiểm thử tính năng Vehicle Type.*
