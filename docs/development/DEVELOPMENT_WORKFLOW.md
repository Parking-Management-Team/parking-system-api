# 🔄 Quy Trình Phát Triển (Development Workflow)

Để duy trì chất lượng mã nguồn ổn định và phối hợp hiệu quả giữa các thành viên, toàn bộ đội ngũ lập trình viên phát triển dự án PBMS cần tuân thủ quy trình dưới đây.

---

## 1. Quy Trình Quản Lý Nhánh Git (Git Workflow)

Dự án áp dụng mô hình Git Flow rút gọn:
* **`main`**: Nhánh chứa mã nguồn chính thức đang chạy trên môi trường Production. Chỉ chấp nhận gộp code từ nhánh `develop` thông qua Release Pull Request được phê duyệt.
* **`develop`**: Nhánh tích hợp chính của đội phát triển. Các tính năng mới sau khi hoàn thành sẽ được gộp vào đây để test tích hợp.
* **`feature/*`**: Các nhánh phát triển chức năng mới (ví dụ: `feature/booking-slot`, `feature/vnpay-integration`). Tạo ra từ nhánh `develop` và merge ngược lại vào `develop` sau khi hoàn thành.
* **`hotfix/*`**: Nhánh sửa lỗi khẩn cấp trực tiếp từ `main`. Sau khi sửa xong sẽ gộp vào cả `main` và `develop`.

---

## 2. Quy Ước Đặt Tên Nhánh (Branch Naming)

Tên nhánh phải mô tả ngắn gọn nội dung công việc và viết thường không dấu, phân cách bằng dấu gạch ngang `-`:
* Thêm chức năng mới: `feature/<tên-chức-năng>` (ví dụ: `feature/create-card`)
* Sửa lỗi: `bugfix/<tên-lỗi>` (ví dụ: `bugfix/vnpay-callback-validation`)
* Tối ưu hóa/Refactor: `refactor/<tên-module>` (ví dụ: `refactor/incidents-api`)
* Nâng cấp hạ tầng/Cài đặt: `chore/<tác-vụ>` (ví dụ: `chore/update-nuget-packages`)

---

## 3. Quy Ước Viết Commit Message

Mỗi commit nên đi theo cấu trúc tiêu chuẩn để dễ theo dõi lịch sử:
```
<type>(<scope>): <mô tả ngắn gọn bằng tiếng Anh hoặc tiếng Việt>
```
* **type** (Loại thay đổi):
  * `feat`: Chức năng mới.
  * `fix`: Sửa lỗi.
  * `refactor`: Thay đổi code nhưng không đổi hành vi nghiệp vụ.
  * `test`: Thêm hoặc sửa Unit Test.
  * `docs`: Cập nhật tài liệu kỹ thuật.
  * `chore`: Cấu hình build tool, cập nhật thư viện NuGet,...
* **Ví dụ:**
  * `feat(payment): tích hợp api kiểm tra trạng thái giao dịch vnpay`
  * `fix(slots): sửa lỗi hiển thị sai trạng thái bãi đỗ xe khi đặt trước`

---

## 4. Quy Trình Pull Request (PR) & Code Review

1. **Trước khi tạo PR:**
   * Hãy chạy thử code ở local để đảm bảo không lỗi compiler.
   * Chạy lệnh chạy test tự động: `dotnet test` và đảm bảo 100% pass.
2. **Tạo PR:**
   * Tạo PR từ nhánh feature của bạn trỏ vào nhánh `develop`.
   * Điền thông tin mô tả PR: Đã thay đổi những gì, lý do thay đổi và cách test thử.
3. **Quy trình Review:**
   * Cần có ít nhất 1 thành viên khác trong đội phát triển (Lead hoặc Peer) review code.
   * Mọi ý kiến đóng góp (Comments) cần được thảo luận và giải quyết triệt để trước khi bấm Merge PR.

---

## 5. Quy Trình Cập Nhật Database (Migration Workflow)

Khi làm việc nhóm, việc xung đột Migration rất dễ xảy ra. Hãy làm theo quy tắc sau:
1. Luôn kéo code mới nhất từ `develop` về trước khi tạo migration mới.
2. Tạo file migration với tên rõ ràng thể hiện đúng thực thể được sửa đổi (ví dụ: `dotnet ef migrations add AddVehicleNormalizedLicensePlate`).
3. Nếu phát hiện có migration mới của thành viên khác vừa được push lên, hãy kéo về (pull) và cập nhật database ở máy local trước khi tiếp tục code.
