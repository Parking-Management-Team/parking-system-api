# 📦 Hướng Dẫn Triển Khai (Deployment Guide)

Tài liệu này hướng dẫn cách build và triển khai ứng dụng PBMS API lên môi trường production sử dụng Docker và CI/CD.

---

## 1. Môi Trường Triển Khai (Target Environments)

* **Production Database:** PostgreSQL hosted on Supabase (Cloud).
* **Application Host:** Máy chủ Cloud (AWS ECS, DigitalOcean App Platform, Azure App Service hoặc VPS Linux cài Docker).
* **CI/CD Platform:** GitHub Actions.

---

## 2. Quy Trình Deploy Thủ Công Bằng Docker

Hệ thống được cấu hình sẵn Dockerfile tại thư mục [docker/Dockerfile](file:///D:/FPT/SWP391/parking-system-api/docker/Dockerfile).

### Bước 1: Build Docker Image
Đứng tại thư mục gốc của dự án (root directory), chạy lệnh build image:
```bash
docker build -f docker/Dockerfile -t pbms-api:latest .
```

### Bước 2: Chạy Container trên Server Production
Khi khởi chạy Container, cần ghi đè các tham số môi trường (Environment Variables) để kết nối trực tiếp đến Database thật và thiết lập JWT bảo mật cao:
```bash
docker run -d \
  -p 8080:5029 \
  --name pbms-api-prod \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=aws-1-ap-southeast-2.pooler.supabase.com;Database=postgres;Username=[REDACTED];Password=[REDACTED];SSL Mode=Require;Trust Server Certificate=true" \
  -e Jwt__Key="[REDACTED]" \
  -e VNPay__HashSecret="[REDACTED]" \
  pbms-api:latest
```

---

## 3. Quy Trình Deploy Tự Động (GitHub Actions CI/CD)

Dự án được cấu hình quy trình CI tự động thông qua file [dotnet-ci.yml](file:///D:/FPT/SWP391/parking-system-api/.github/workflows/dotnet-ci.yml).

### Các bước hoạt động của CI:
1. Kích hoạt khi có hành động `push` hoặc tạo `pull_request` vào các nhánh `main`, `develop`.
2. Khởi động môi trường ảo chạy Ubuntu.
3. Cài đặt môi trường .NET SDK 10.0.
4. Chạy lệnh `dotnet restore` để khôi phục dependencies.
5. Biên dịch toàn bộ source code ở cấu hình `Release`.
6. Chạy các bài Unit Test tự động để đảm bảo chất lượng code trước khi deploy.

> [!TODO]
> Cần bổ sung thêm các bước **CD** (Continuous Deployment) vào file GitHub Actions để tự động build Docker image và đẩy (push) lên Docker Registry (như Docker Hub, AWS ECR) và kích hoạt lệnh cập nhật service trên host server sau khi các bài test CI đã pass.

---

## 4. Các Lưu Ý Quan Trọng Khi Deploy Production

1. **HTTPS bắt buộc:** Đảm bảo toàn bộ traffic kết nối tới API đều đi qua giao thức HTTPS (Cấu hình SSL thông qua Nginx Reverse Proxy hoặc Cloudflare CDN).
2. **Khóa bảo mật JWT:** Giá trị `Jwt:Key` trên môi trường Production phải là một chuỗi ngẫu nhiên có độ dài tối thiểu 256-bit và không được trùng với Key dùng ở môi trường test/local.
3. **VNPay Sandbox vs Production:**
   * Khi chuyển sang môi trường Production của VNPay, hãy cập nhật `VNPay:BaseUrl` sang endpoint thật và xin cấp mới cặp mã `TmnCode` & `HashSecret` thật từ VNPay.
4. **Không chạy Migrations tự động trên Production:** Tránh sử dụng `EnsureCreated()` trên DB Production để không làm mất mát cấu trúc dữ liệu cũ. Hãy quản lý cấu trúc DB production thông qua các script migration được kiểm duyệt kỹ càng.
