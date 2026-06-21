# 📚 Tài Liệu Kỹ Thuật Nội Bộ - PBMS API

Chào mừng bạn đến với thư mục tài liệu kỹ thuật nội bộ của dự án **Parking Building Management System (PBMS) API**. Tài liệu này được xây dựng nhằm hỗ trợ quá trình phát triển, bảo trì, vận hành hệ thống và onboarding thành viên mới.

---

## 🗂️ Cấu Trúc Bộ Tài Liệu

Bộ tài liệu được tổ chức thành các phần chuyên biệt dưới đây:

| Tệp tài liệu | Nội dung chính |
| :--- | :--- |
| 🌍 [TỔNG QUAN HỆ THỐNG](SYSTEM_OVERVIEW.md) | Mục tiêu dự án, danh sách chức năng chính, các module, công nghệ sử dụng và kiến trúc tổng quan. |
| 🏗️ [KIẾN TRÚC HỆ THỐNG](ARCHITECTURE.md) | Phân tích chi tiết Clean Architecture, luồng phụ thuộc (Dependency flow), và trách nhiệm của từng Layer (`API`, `Application`, `Domain`, `Infrastructure`). |
| 🚀 [HƯỚNG DẪN CÀI ĐẶT](SETUP_GUIDE.md) | Yêu cầu hệ thống, các bước thiết lập môi trường phát triển local, restore packages, migration database và chạy ứng dụng. |
| ⚙️ [CẤU HÌNH HỆ THỐNG](CONFIGURATION.md) | Chi tiết các tham số cấu hình, cổng chạy (localhost ports), biến môi trường (Environment Variables), JWT key, tích hợp VNPay và Google OAuth. |
| 🗄️ [THIẾT KẾ CƠ SỞ DỮ LIỆU](DATABASE.md) | Lược đồ DB, các Entity chính, quan hệ giữa các bảng, cấu hình migrations, dữ liệu hạt giống (Seed data). |
| 🔌 [HƯỚNG DẪN API](API_GUIDE.md) | Danh sách các controller, quy ước endpoints, cơ chế xác thực JWT, định dạng request/response chuẩn. |
| 📦 [HƯỚNG DẪN TRIỂN KHAI](DEPLOYMENT_GUIDE.md) | Hướng dẫn Dockerize, quy trình CI/CD với GitHub Actions và các lưu ý khi deploy production. |
| 🔄 [QUY TRÌNH PHÁT TRIỂN](DEVELOPMENT_WORKFLOW.md) | Quy chuẩn Git workflow, đặt tên nhánh (branch naming), quy ước commit, quy trình Pull Request và cập nhật DB Migrations. |
| 🛠️ [XỬ LÝ SỰ CỐ](TROUBLESHOOTING.md) | Tổng hợp các lỗi phổ biến thường gặp trong quá trình cài đặt, chạy local và cách xử lý nhanh. |
| 💡 [CƠ SỞ TRI THỨC (KB)](KNOWLEDGE_BASE.md) | Các tài khoản test, liên kết hữu ích, các tập lệnh thường dùng và cẩm nang lưu ý cho lập trình viên mới. |

---

## 🎯 Lưu Ý Cho Developer Mới

1. Hãy bắt đầu bằng cách đọc kỹ [TỔNG QUAN HỆ THỐNG](SYSTEM_OVERVIEW.md) để hiểu nghiệp vụ bãi đỗ xe thông minh.
2. Làm theo từng bước trong [HƯỚNG DẪN CÀI ĐẶT](SETUP_GUIDE.md) để thiết lập dự án chạy được trên máy của bạn.
3. Đảm bảo tuân thủ nghiêm ngặt quy trình làm việc được định nghĩa trong [QUY TRÌNH PHÁT TRIỂN](DEVELOPMENT_WORKFLOW.md) trước khi tạo bất kỳ Pull Request nào.
