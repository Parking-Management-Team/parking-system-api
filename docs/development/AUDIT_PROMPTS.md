# 💡 Audit Prompts

Tài liệu này lưu trữ danh sách các prompt mẫu được thiết kế chuẩn chỉnh để hỗ trợ việc chạy đánh giá, rà soát và kiểm toán (audit) dự án PBMS API định kỳ hoặc sau mỗi chu kỳ phát triển lớn.

---

## 1. Architecture Review Prompt (Rà soát kiến trúc)

* **Mục tiêu:** Đánh giá độ sạch của mã nguồn, kiểm tra xem có sự vi phạm luồng phụ thuộc giữa các tầng (Clean Architecture), vi phạm SOLID hay rò rỉ công nghệ không.
* **Khi nào nên chạy:** Sau mỗi sprint phát triển hoặc trước khi chuẩn bị release phiên bản lớn.
* **Input cần thiết:** Toàn bộ cấu trúc thư mục dự án Backend, các tệp cấu hình `.csproj` của từng layer, tệp `Program.cs` và `DependencyInjection.cs`.
* **Output mong đợi:** 
Trước khi ghi nội dung vào file docs/API_READINESS_AUDIT.md,
chạy lệnh `git rev-parse --short HEAD` và `git log -1 --format=%ad --date=short`
để lấy commit hash + ngày hiện tại, rồi chèn dòng:
"> 🔍 Audited at commit: `<hash>` — <date>"
ngay dòng đầu tiên của file.
Báo cáo chỉ ra các điểm vi phạm cấu trúc, rò rỉ dependencies và danh sách khuyến nghị refactor kèm độ ưu tiên.

### 📝 Prompt mẫu:
```markdown
Đóng vai trò là một Senior .NET Architect, hãy quét qua toàn bộ cấu trúc dự án và các tệp Dependency Injection hiện tại. Phân tích xem:
1. Có class nào ở tầng Domain hay Application đang tham chiếu trực tiếp đến các thư viện ngoài như Entity Framework Core hay Npgsql hay không?
2. Sự phụ thuộc giữa các project có tuân thủ nghiêm ngặt quy tắc luồng đi từ ngoài vào trong hay không?
3. Hãy chỉ ra các đoạn code vi phạm nguyên lý Single Responsibility (SRP) hoặc Dependency Inversion (DIP) và đưa ra giải pháp khắc phục.
```

---

## 2. API Readiness Audit Prompt (Đánh giá độ sẵn sàng của API)

* **Mục tiêu:** Rà soát kỹ lưỡng các API Endpoints xem có cung cấp đầy đủ thông tin, logic nghiệp vụ, validation đầu vào và khả năng tích hợp mượt mà với Frontend hay không.
* **Khi nào nên chạy:** Khi bắt đầu bàn giao API cho đội Frontend tích hợp hoặc khi có phản hồi lỗi từ FE.
* **Input cần thiết:** Controllers source code, DTOs, tệp `API_INVENTORY.md` và `API_GUIDE.md`.
* **Output mong đợi:** 
Trước khi ghi nội dung vào file docs/API_READINESS_AUDIT.md,
chạy lệnh `git rev-parse --short HEAD` và `git log -1 --format=%ad --date=short`
để lấy commit hash + ngày hiện tại, rồi chèn dòng:
"> 🔍 Audited at commit: `<hash>` — <date>"
ngay dòng đầu tiên của file.
Báo cáo chi tiết từng API bị thiếu thông tin hoặc thiếu logic nghiệp vụ cần thiết cho giao diện người dùng.

### 📝 Prompt mẫu:
```markdown
Đóng vai trò là một Frontend Architect kiêm Product Analyst, hãy quét toàn bộ các API Controller hiện tại. Đối với mỗi endpoint, hãy đánh giá:
1. Dữ liệu trả về (Response Payload) đã đủ để Frontend hiển thị giao diện hay chưa? Có bị thiếu các trường thông tin liên kết quan trọng không?
2. Logic kiểm tra validation đầu vào (Request validation) đã chặt chẽ chưa? Có trả về thông báo lỗi chi tiết giúp Frontend hiển thị thông báo lỗi thân thiện cho user không?
3. Gắn nhãn đánh giá độ sẵn sàng (Ready / Minor / Significant Improvements) kèm độ ưu tiên sửa đổi (P0/P1/P2).
```

---

## 3. Dead API Audit Prompt (Tìm kiếm API rác/không sử dụng)

* **Mục tiêu:** Phát hiện các API Endpoints được sinh ra hoặc để thừa trong quá trình dev nhưng không còn sử dụng trong thực tế, giúp làm sạch mã nguồn.
* **Khi nào nên chạy:** Trước khi dọn dẹp hệ thống chuẩn bị đưa lên môi trường Staging/Production.
* **Input cần thiết:** Các file Controller hiện có và lịch sử gọi API (nếu có log) hoặc mã nguồn Frontend (để tìm kiếm đối chiếu).
* **Output mong đợi:** 
Trước khi ghi nội dung vào file docs/API_READINESS_AUDIT.md,
chạy lệnh `git rev-parse --short HEAD` và `git log -1 --format=%ad --date=short`
để lấy commit hash + ngày hiện tại, rồi chèn dòng:
"> 🔍 Audited at commit: `<hash>` — <date>"
ngay dòng đầu tiên của file.
Danh sách các endpoint không có class nào sử dụng hoặc không được Frontend gọi tới để tiến hành lưu trữ hoặc xóa bỏ.

### 📝 Prompt mẫu:
```markdown
Hãy đối chiếu danh sách các API endpoints trong các Controllers của Backend với mã nguồn gọi API của dự án Frontend (hoặc tệp định nghĩa gọi dịch vụ API). Chỉ ra:
1. Những API nào được khai báo ở Backend nhưng Frontend hoàn toàn không gọi tới?
2. Những API nào không còn phục vụ bất kỳ Use Case nào trong SRS (Tài liệu đặc tả yêu cầu)?
3. Đề xuất giữ lại hay loại bỏ để tối ưu hóa hiệu năng hệ thống.
```

---

## 4. Frontend Integration Audit Prompt (Đánh giá tích hợp Frontend)

* **Mục tiêu:** Đảm bảo cấu trúc phản hồi lỗi, cơ chế phân trang và định dạng ngày tháng tương thích tuyệt đối giữa FE và BE.
* **Khi nào nên chạy:** Trước khi bắt đầu ghép nối các màn hình nghiệp vụ phức tạp (như thanh toán, check-in).
* **Input cần thiết:** BaseResponse structure, các Middleware xử lý lỗi và các DTOs phân trang.
* **Output mong đợi:** Danh sách các điểm mâu thuẫn về kiểu dữ liệu hoặc cấu trúc JSON phản hồi giữa 2 bên.

### 📝 Prompt mẫu:
```markdown
Hãy phân tích cấu trúc BaseResponse của hệ thống. Kiểm tra xem:
1. Định dạng ngày tháng trả về có đồng bộ là UTC (ISO 8601) hay không?
2. Định dạng phân trang (Pagination DTO) có chứa đầy đủ thông tin (PageIndex, PageSize, TotalCount, TotalPages) để Frontend hiển thị thanh phân trang hay không?
3. Khi có lỗi Validation (HTTP 400), cấu trúc lỗi trả về có dễ dàng để Frontend ánh xạ (mapping) và hiển thị tương ứng vào từng ô nhập liệu bị lỗi không?
```

---

## 5. Security Audit Prompt (Kiểm toán bảo mật API)

* **Mục tiêu:** Kiểm tra các lỗ hổng rò rỉ thông tin nhạy cảm, thiếu token xác thực JWT hoặc lỗi phân quyền vai trò người dùng (Broken Object Level Authorization).
* **Khi nào nên chạy:** Bắt buộc phải chạy trước khi đưa hệ thống lên Production hoặc Staging.
* **Input cần thiết:** Mã nguồn Controllers, cấu hình JWT Bearer trong `Program.cs` và các annotation kiểm tra quyền truy cập.
* **Output mong đợi:** Danh sách các lỗ hổng bảo mật được xếp hạng nguy hiểm (High/Medium/Low) kèm mã lỗi bảo mật.

### 📝 Prompt mẫu:
```markdown
Đóng vai trò là một Chuyên gia bảo mật ứng dụng (Application Security Engineer), hãy phân tích bảo mật hệ thống API:
1. Có endpoint nhạy cảm nào (sửa thông tin, thanh toán, cấu hình giá, xem log) thiếu attribute [Authorize] hoặc [Authorize(Roles = ...)] không?
2. Có nguy cơ IDOR (Insecure Direct Object Reference) nào trong các API dạng GET/PUT/DELETE theo ID (ví dụ: tài khoản thường có thể xem hoặc sửa thông tin tài khoản khác bằng cách thay đổi ID trên URL) hay không?
3. Hệ thống có đang trả về thông tin nhạy cảm (như mật khẩu băm, token cũ, thông tin liên lạc nội bộ) trong response payload hay không?
```

---

## 6. Dependency Audit Prompt (Kiểm tra thư viện phụ thuộc)

* **Mục tiêu:** Rà soát các gói package NuGet xem có bị lỗi thời, chứa lỗ hổng bảo mật đã được công bố hoặc dư thừa không cần thiết hay không.
* **Khi nào nên chạy:** Định kỳ hàng tháng hoặc trước khi nâng cấp phiên bản .NET SDK.
* **Input cần thiết:** Các file `.csproj` trong toàn bộ các projects của solution.
* **Output mong đợi:** Khuyến nghị nâng cấp hoặc gỡ bỏ các thư viện cũ.

### 📝 Prompt mẫu:
```markdown
Hãy rà soát toàn bộ các gói thư viện (Nuget PackageReferences) được khai báo trong các tệp .csproj của dự án. Hãy phân tích:
1. Có thư viện nào đã lỗi thời hoặc chứa lỗ hổng bảo mật nghiêm trọng (CVE) đã được công bố không?
2. Có sự chồng chéo hoặc thừa thãi package nào giữa các dự án con (như việc import thư viện ở lớp API nhưng lớp Domain lại không dùng) hay không?
3. Đưa ra danh sách nâng cấp package an toàn cho dự án .NET 10.0 hiện tại.
```

---

## 7. Database Design Audit Prompt (Đánh giá thiết kế cơ sở dữ liệu)

* **Mục tiêu:** Đánh giá cấu trúc lược đồ bảng (Schema DB), các quan hệ khóa ngoại, hiệu năng truy vấn (Index) và logic seed data.
* **Khi nào nên chạy:** Khi có sự thay đổi lớn về Entity Domain hoặc hệ thống gặp hiện tượng truy vấn chậm (Slow query).
* **Input cần thiết:** Domain Entities, AppDbContext, configurations Fluent API và các tệp Migrations.
* **Output mong đợi:** Đề xuất tối ưu hóa bảng, tạo index thích hợp và cải thiện hiệu năng truy xuất DB.

### 📝 Prompt mẫu:
```markdown
Hãy đóng vai trò là một Database Architect chuyên về PostgreSQL. Phân tích thiết kế database thông qua các Entity Domain và Configurations:
1. Các quan hệ 1-N hoặc N-N đã được cấu hình khóa ngoại đầy đủ chưa? Có nguy cơ xảy ra lỗi Cascade Delete làm mất dữ liệu liên đới ngoài ý muốn không?
2. Dựa vào các nghiệp vụ truy vấn chính (như tìm kiếm xe, tìm kiếm thẻ bằng mã code, lọc phiên đỗ xe active), những trường dữ liệu nào cần được tạo Index để tối ưu hiệu năng?
3. Logic Soft Delete (ISoftDeletable) đã được cấu hình tự động (Global Query Filter) trong DbContext chưa hay lập trình viên đang phải viết filter thủ công ở mọi nơi?
```
