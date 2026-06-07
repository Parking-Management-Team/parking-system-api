# 3. Stakeholders & Actors

## 3.1 Stakeholders

| Stakeholder            | Vai trò                                                     |
|------------------------|-------------------------------------------------------------|
| Chủ bãi xe / Chủ dự án | Muốn hệ thống giúp quản lý vận hành và doanh thu.           |
| Parking Manager        | Theo dõi hoạt động bãi xe, cấu hình bãi, bảng giá, báo cáo. |
| Parking Staff          | Thực hiện nghiệp vụ xe vào/ra hằng ngày.                    |
| Parking User / Driver  | Người gửi xe, booking, thanh toán, đăng ký thẻ tháng.       |
| System Administrator   | Quản lý tài khoản, phân quyền, cấu hình hệ thống.           |
| Development Team       | Phân tích, thiết kế, triển khai hệ thống.                   |
| Testing Team           | Kiểm thử chức năng và luồng nghiệp vụ.                      |

---

## 3.2 Actors

| Actor                | Mô tả                                                                          |
|----------------------|--------------------------------------------------------------------------------|
| Parking Manager      | Người quản lý vận hành bãi xe.                                                 |
| Parking Staff        | Nhân viên thao tác nghiệp vụ tại cổng hoặc quầy.                               |
| Driver               | Người gửi xe, đặt chỗ, đăng ký thẻ tháng, thanh toán.                          |
| System Administrator | Người quản trị hệ thống.                                                       |
| Bank Payment Gateway | Hệ thống thanh toán online qua ngân hàng.                                      |
| System               | Tự động kiểm tra chỗ trống, trạng thái booking, tính phí, cập nhật trạng thái. |

---

## 3.3 Actor Permission Overview

| Actor                | Main Permissions                                                         |
|----------------------|--------------------------------------------------------------------------|
| Parking Manager      | Quản lý cấu trúc bãi xe, bảng giá, xem tình trạng vận hành, xem báo cáo. |
| Parking Staff        | Check-in, check-out, nhập biển số, tạo session, thu phí, xử lý ngoại lệ. |
| Driver               | Xem thông tin bãi, booking, đăng ký thẻ tháng, xem lượt gửi, thanh toán. |
| System Administrator | Quản lý tài khoản, phân quyền, cấu hình chung.                           |
| Bank Payment Gateway | Xác nhận kết quả thanh toán online.                                      |

---
