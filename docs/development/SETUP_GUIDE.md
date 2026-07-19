# 🚀 Hướng Dẫn Cài Đặt (Setup Guide)

Tài liệu này hướng dẫn các bước chi tiết để thiết lập môi trường phát triển local và chạy dự án PBMS API.

---

## 1. Yêu Cầu Hệ Thống (Prerequisites)

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt các phần mềm sau:
* **.NET 10.0 SDK** (bắt buộc)
* **Git** (để quản lý phiên bản)
* **PostgreSQL** (bắt buộc - chạy cục bộ bằng Docker hoặc cài đặt trực tiếp trên hệ điều hành).
* **IDE/Editor:** Visual Studio 2022 (v17.12 trở lên) hoặc Visual Studio Code (với C# Dev Kit), JetBrains Rider.
* **Docker / Docker Desktop** (tùy chọn - khuyên dùng để chạy nhanh PostgreSQL local).

---

## 2. Các Bước Thiết Lập (Installation Steps)

### Bước 1: Clone Project
Mở Terminal hoặc CMD/PowerShell và clone project về máy:
```bash
git clone <repository_url>
cd parking-system-api
```

### Bước 2: Khôi phục Dependencies (Restore packages)
Tải các thư viện NuGet cần thiết về máy:
```bash
dotnet restore
```

### Bước 3: Build dự án
Đảm bảo mã nguồn biên dịch thành công mà không gặp lỗi:
```bash
dotnet build
```

---

## 3. Cấu Hình Cơ Sở Dữ Liệu (Database Setup)

Dự án sử dụng cơ sở dữ liệu **PostgreSQL** cho cả môi trường phát triển (Development) và vận hành thực tế (Production).

### Khởi động PostgreSQL Local nhanh bằng Docker (Khuyên dùng):
Bạn có thể khởi động một container PostgreSQL local bằng lệnh sau:
```bash
docker run --name pbms-postgres -e POSTGRES_PASSWORD=123456 -e POSTGRES_DB=pbms_local -p 5432:5432 -d postgres:latest
```

### Cấu hình chuỗi kết nối:
Chi tiết các cổng chạy và chuỗi kết nối DB được định nghĩa tại tài liệu [CẤU HÌNH HỆ THỐNG](CONFIGURATION.md). Đảm bảo thông tin kết nối trong `appsettings.Development.json` khớp với thông tin DB của bạn.

### Áp dụng Migrations để tạo bảng:
Cài đặt công cụ Entity Framework Core CLI nếu chưa có:
```bash
dotnet tool install --global dotnet-ef
```

Sau đó chạy lệnh cập nhật cấu trúc database:
```bash
dotnet ef database update --project src/PBMS.Infrastructure --startup-project src/PBMS.API
```

---

## 4. Chạy Dự Án Local (Running Project)

### Chạy bằng CLI
Khởi động API ở môi trường phát triển:
```bash
dotnet run --project src/PBMS.API
```

### Chạy bằng Visual Studio hoặc Rider
1. Mở file solution `PBMS.slnx` hoặc thư mục dự án bằng IDE của bạn.
2. Chọn Startup Project là `PBMS.API`.
3. Chọn profile chạy (`Local` hoặc `Supabase`) từ danh sách Run configurations.
4. Nhấn phím `F5` hoặc nút `Play` để chạy debug.

---

## 5. Kiểm Tra Hệ Thống Hoạt Động (Verification)

Sau khi API khởi chạy thành công, hãy mở trình duyệt web và truy cập:
* **Swagger UI:** [http://localhost:5029/swagger](http://localhost:5029/swagger) (Xem hướng dẫn kiểm thử chi tiết tại [HƯỚNG DẪN API](API_GUIDE.md)).
