# Table of Contents

- [1. Introduction](#1-introduction)
  - [1.1 Purpose](#11-purpose)
  - [1.2 Scope](#12-scope)
  - [1.3 Definitions, Acronyms, Abbreviations](#13-definitions-acronyms-abbreviations)
  - [1.4 References](#14-references)
  - [1.5 Document Overview](#15-document-overview)

- [2. Overall Description](#2-overall-description)
  - [2.1 Product Perspective](#21-product-perspective)
  - [2.2 Product Functions](#22-product-functions)
  - [2.3 User Classes and Characteristics](#23-user-classes-and-characteristics)
  - [2.4 Operating Environment](#24-operating-environment)
  - [2.5 Constraints](#25-constraints)
  - [2.6 Assumptions and Dependencies](#26-assumptions-and-dependencies)

- [3. Stakeholders & Actors](#3-stakeholders--actors)
  - [3.1 Stakeholders](#31-stakeholders)
  - [3.2 Actors](#32-actors)
  - [3.3 Actor Permission Overview](#33-actor-permission-overview)

- [4. Business Context](#4-business-context)
  - [4.1 Problem Statement](#41-problem-statement)
  - [4.2 Business Goals](#42-business-goals)
  - [4.3 Success Criteria](#43-success-criteria)
  - [4.4 Current Workflow](#44-current-workflow)
  - [4.5 Target Workflow](#45-target-workflow)
    - [4.5.1 Booking Workflow](#451-booking-workflow)
    - [4.5.2 Monthly Subscription Workflow](#452-monthly-subscription-workflow)
    - [4.5.3 Check-in Workflow](#453-check-in-workflow)
    - [4.5.4 Check-out & Payment Workflow](#454-check-out--payment-workflow)

- [5. Functional Requirements](#5-functional-requirements)
  - [Feature List](#feature-list)
  - [FR-001: Parking Structure Management](#fr-001-parking-structure-management)
  - [FR-002: Driver Account & Vehicle Management](#fr-002-driver-account--vehicle-management)
  - [FR-003: Vehicle Check-in](#fr-003-vehicle-check-in)
  - [FR-004: Parking Allocation](#fr-004-parking-allocation)
  - [FR-005: Booking Management](#fr-005-booking-management)
  - [FR-006: Monthly Subscription Management](#fr-006-monthly-subscription-management)
  - [FR-007: Parking Session Tracking](#fr-007-parking-session-tracking)
  - [FR-008: Vehicle Check-out](#fr-008-vehicle-check-out)
  - [FR-009: Payment Management](#fr-009-payment-management)
  - [FR-010: Fee Calculation](#fr-010-fee-calculation)
  - [FR-011: Exception Handling](#fr-011-exception-handling)
  - [FR-012: Operation Monitoring](#fr-012-operation-monitoring)

- [6. Business Rules](#6-business-rules)
  - [6.1 Parking Structure Rules](#61-parking-structure-rules)
  - [6.2 Hardware Simulation Rules](#62-hardware-simulation-rules)
  - [6.3 Driver Account & Vehicle Rules](#63-driver-account--vehicle-rules)
  - [6.4 Parking Allocation Rules](#64-parking-allocation-rules)
  - [6.5 Booking Rules](#65-booking-rules)
  - [6.6 Monthly Subscription Rules](#66-monthly-subscription-rules)
  - [6.7 Payment Rules](#67-payment-rules)
  - [6.8 Fee Calculation Rules](#68-fee-calculation-rules)
  - [6.9 Parking Session Rules](#69-parking-session-rules)
  - [6.10 Vehicle Check-out Rules](#610-vehicle-check-out-rules)
  - [6.11 Exception Handling Rules](#611-exception-handling-rules)
  - [6.12 Operation Monitoring Rules](#612-operation-monitoring-rules)
  - [6.13 System State Rules](#613-system-state-rules)
    - [Permission Status](#permission-status)
    - [Account Status](#account-status)
    - [Building Status](#building-status)
    - [Floor Status](#floor-status)
    - [Zone Status](#zone-status)
    - [Slot Status](#slot-status)
    - [Vehicle Type Status](#vehicle-type-status)
    - [Vehicle Status](#vehicle-status)
    - [Card Status](#card-status)
    - [Parking Session Status](#parking-session-status)
    - [Booking Status](#booking-status)
    - [Incident Status](#incident-status)
    - [Monthly Subscription Status](#monthly-subscription-status)
    - [Pricing Policy Status](#pricing-policy-status)
  - [6.14 Configurable Variables Rules](#614-configurable-variables-rules)
  - [6.15 Business Rules Summary](#615-business-rules-summary)

- [7. Finalized Policy Decisions](#7-finalized-policy-decisions)

- [8. Concept, Entity & Physical Model](#8-concept-entity--physical-model)
  - [8.1 Modeling Scope](#81-modeling-scope)
  - [8.2 Entity Summary](#82-entity-summary)
  - [8.3 Physical Model Normalized](#83-physical-model-normalized)
    - [8.3.1 Modeling Rules](#831-modeling-rules)
      - [8.3.1.1 Database-Agnostic Rule](#8311-database-agnostic-rule)
      - [8.3.1.2 Naming Convention](#8312-naming-convention)
      - [8.3.1.3 Allowed Data Types](#8313-allowed-data-types)
      - [8.3.1.4 Generic Constraints](#8314-generic-constraints)
    - [8.3.2 Relationship Summary From Conceptual Model](#832-relationship-summary-from-conceptual-model)
    - [8.3.3 Physical Tables](#833-physical-tables)
    - [8.3.4 Mermaid ERD With Physical Tables](#834-mermaid-erd-with-physical-tables)

---
# 1. Introduction

## 1.1 Purpose

Tài liệu này mô tả yêu cầu nghiệp vụ và yêu cầu chức năng của hệ thống quản lý tòa nhà gửi xe.

Mục tiêu của tài liệu là giúp team hiểu rõ:

- Hệ thống phục vụ ai.
- Hệ thống giải quyết vấn đề gì.
- Hệ thống có những chức năng nào.
- Luồng xử lý xe máy, ô tô, booking, thẻ tháng và thanh toán diễn ra như thế nào.
- Các business rule chính cần tuân theo khi triển khai.

Tài liệu này có thể dùng làm cơ sở cho BA, developer, tester, PM và stakeholder khi phân tích, phát triển, kiểm thử và
nghiệm thu hệ thống.

---

## 1.2 Scope

### In Scope

| Nhóm phạm vi                      | Nội dung                                                                                                                                                                                                                                                                         |
|-----------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Hardware Simulation               | Không có camera, thẻ vật lý, barrier, cảm biến slot. Tất cả thao tác được nhập hoặc xác nhận thủ công trên web.                                                                                                                                                                  |
| Vehicle Support                   | Hệ thống cho phép cấu hình loại phương tiện ở mức dữ liệu nghiệp vụ. Trong phiên bản hiện tại, hệ thống chỉ kích hoạt và kiểm thử chính cho xe máy và ô tô.<br/>Các loại phương tiện khác như xe đạp, xe điện có thể được bổ sung sau thông qua cấu hình hoặc mở rộng nghiệp vụ. |
| Motorcycle Parking                | Khi check-in, hệ thống hiện gợi ý Zone/Area còn chỗ, không quản lý tới từng slot cụ thể.                                                                                            |
| Car Parking                       | Khi check-in, ô tô Walk-in/Booking chỉ được gán Slot trong Zone `GENERAL`; ô tô thẻ tháng dùng Slot riêng trong Zone `MONTHLY`. |
| Booking                           | Người dùng có thể đặt trước chỗ gửi xe, bắt buộc nhập biển số và đặt cọc phí booking.                                                                                                                                                                                            |
| Motorcycle Booking                | Xe máy booking không chọn Zone/Slot cụ thể. Hệ thống đảm bảo có chỗ khi khách đến đúng thời gian hợp lệ.                                                                                                                                                                         |
| Car Booking                       | Ô tô booking chỉ chọn Building và Vehicle/biển số; hệ thống giữ general capacity ở Building và gán Slot trong Zone `GENERAL` khi check-in. |
| Monthly Subscription              | Hồ sơ đăng ký/thẻ tháng gắn với Vehicle, Building và Card `MONTHLY`; xe máy giữ capacity động, ô tô giữ Slot riêng bằng `monthly_subscription.assigned_slot_id`. |
| Payment                           | Hỗ trợ thanh toán tiền mặt và thanh toán online thật qua ngân hàng. Không hỗ trợ thanh toán bằng thẻ.                                                                                                                                                                            |
| Fee Calculation                   | Giá gửi xe được tính theo mô hình Time Window, Base Price, Increment/Block Pricing và Window Cap. Nếu phiên gửi xe đi qua nhiều khung giờ, hệ thống tách phiên theo từng khung giờ và áp dụng bảng giá riêng cho từng khung.                                                     |
| Pricing Policy Configuration      | Manager có thể cấu hình bảng giá theo loại xe, khung giờ, thời lượng cơ bản, giá cơ bản, block tính thêm, giá block phát sinh, cap theo khung giờ, phí phạt và các biến cấu hình liên quan.                                                                                      |
| Rounding Policy                   | Hệ thống hỗ trợ rule làm tròn thời gian phát sinh theo grace period và làm tròn tiền mặt theo đơn vị làm tròn cấu hình. Thanh toán online giữ nguyên giá trị chính xác.                                                                                                          |
| System State Management           | Hệ thống quản lý trạng thái Building, Floor, Zone, Slot, Vehicle, Card, Parking Session, Booking, Incident, Monthly Subscription và Pricing Policy theo các trạng thái nghiệp vụ đã định nghĩa.                                                                                          |
| Driver Account & Vehicle          | Tài khoản Driver có thể được tạo trước khi có xe. Một tài khoản có thể thêm nhiều xe sau này.                                                                                                                                                                                    |
| Scalability in Business Structure | Có thể mở rộng Building, Floor, Zone, Slot ở mức cấu hình nghiệp vụ.                                                                                                                                                                                                             |
| Parking Session Tracking          | Ghi nhận xe vào, trạng thái đang gửi, khu vực/slot được phân bổ, phí tạm tính.                                                                                                                                                                                                   |
| Concept / Entity / Physical Model | Tài liệu bao gồm phần tổng quan concept relationship, entity summary và physical table model để đồng bộ nghiệp vụ với database model.                                                                                                                                             |
| Vehicle Check-out                 | Staff tìm lượt gửi xe, xác nhận xe ra, tính phí, thu tiền.                                                                                                                                                                                                                       |
| Exception Handling                | Mất mã gửi xe/thẻ mô phỏng, sai biển số, quá hạn, gửi sai khu vực, chưa thanh toán.                                                                                                                                                                                              |
| Operation Monitoring              | Manager theo dõi slot/zone, bảng giá, lượt xe, doanh thu, tỷ lệ lấp đầy.                                                                                                                                                                                                         |

### Out of Scope

| Nội dung                      | Lý do                                                              |
|-------------------------------|--------------------------------------------------------------------|
| Camera nhận diện biển số thật | Không có hardware thực tế.                                         |
| Thẻ xe vật lý thật            | Mã thẻ/mã gửi xe chỉ là mô phỏng trên web.                         |
| Barrier tự động               | Không tích hợp thiết bị cổng thật.                                 |
| Cảm biến slot thật            | Trạng thái chỗ đỗ được cập nhật bằng thao tác hệ thống/người dùng. |
| Thanh toán bằng thẻ ngân hàng | Chỉ hỗ trợ thanh toán online qua ngân hàng, không thanh toán thẻ.  |
| NFR chi tiết                  | Không phân tích trong phạm vi tài liệu này.                        |
| AI allocation nâng cao        | Có thể để optional/RBL, chưa đưa vào core scope.                   |

---

## 1.3 Definitions, Acronyms, Abbreviations

| Thuật ngữ               | Ý nghĩa trong hệ thống này                                                                                                                            |
|-------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------|
| Building                | Một tòa nhà gửi xe.                                                                                                                                   |
| Floor                   | Một tầng trong tòa nhà.                                                                                                                               |
| Zone/Area               | Khu vực đỗ xe, dùng chính cho xe máy.                                                                                                                 |
| Slot                    | Vị trí đỗ cụ thể, dùng chính cho ô tô.                                                                                                                |
| Parking Session         | Một lượt gửi xe từ lúc check-in đến check-out.                                                                                                        |
| Booking                 | Đặt chỗ trước cho một khoảng thời gian.                                                                                                               |
| Monthly Subscription    | Hồ sơ đăng ký và quyền lợi gửi xe định kỳ gắn với một Vehicle; tên nghiệp vụ tiếng Việt có thể là thẻ tháng/vé tháng. |
| Card                    | Card vận hành do bãi xe quản lý, dùng để nhận diện một Parking Session bằng `card_code` hiện tại hoặc `nfc_uid` trong tương lai. |
| Manual Input            | Người dùng nhập dữ liệu bằng tay trên web, thay cho camera/thẻ/hardware thật.                                                                         |
| Pricing Window          | Khung thời gian áp dụng một bảng giá cụ thể, ví dụ khung ngày hoặc khung đêm.                                                                         |
| Base Duration           | Thời lượng cơ bản được tính theo giá cơ bản.                                                                                                          |
| Base Price              | Giá cơ bản áp dụng cho Base Duration.                                                                                                                 |
| Increment Block         | Đơn vị thời gian tính thêm sau khi vượt Base Duration.                                                                                                |
| Increment Price         | Giá phát sinh cho mỗi Increment Block.                                                                                                                |
| Window Cap              | Mức phí tối đa trong một khung giờ riêng biệt. Không áp dụng cho toàn bộ phiên gửi xe.                                                                |
| Grace Period            | Khoảng thời gian ân hạn. Nếu thời gian phát sinh nhỏ hơn hoặc bằng grace period thì không tính thêm block mới.                                        |
| Deposit Fee             | Khoản phí khách phải thanh toán trước khi booking được xác nhận. Theo Parking Price, Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành. |
| Payment Timeout         | Thời gian tối đa cho phép chờ thanh toán booking. Nếu quá thời gian này mà chưa thanh toán thành công, booking bị hủy.                                |
| Check-in Grace Time     | Thời gian ân hạn cho phép khách check-in sau giờ booking đã xác nhận.                                                                                 |
| Downgrade               | Việc chuyển vé tháng hết hạn sang trạng thái tính phí như khách vãng lai nếu xe vẫn còn trong bãi.                                                    |
| Cash Rounding           | Quy tắc làm tròn số tiền khi thanh toán tiền mặt.                                                                                                     |
| Online Payment Rounding | Thanh toán online không làm tròn, giữ nguyên giá trị chính xác.                                                                                       |
| Card Code               | Mã nghiệp vụ của Card để Staff nhập thủ công khi check-in/check-out, ví dụ `CARD-000001`. |
| NFC UID                 | UID kỹ thuật của chip NFC, chỉ dùng khi hệ thống tích hợp Card NFC trong tương lai. |
| Available               | Còn trống.                                                                                                                                            |
| Occupied                | Đang có xe sử dụng.                                                                                                                                   |
| Assigned                | Đã được gán cho session hoặc subscription theo quan hệ nghiệp vụ tương ứng. |
| Maintenance/Locked      | Tạm khóa, không được phân bổ.                                                                                                                         |

---

## 1.4 Document Overview

- Chương 1 mô tả mục đích, phạm vi, thuật ngữ và tài liệu tham chiếu.
- Chương 2 mô tả tổng quan hệ thống.
- Chương 3 mô tả stakeholders, actors và quyền tổng quan.
- Chương 4 mô tả business context, workflow và success criteria.
- Chương 5 mô tả functional requirements.
- Chương 6 mô tả business rules.
- Chương 7 mô tả các điểm chính sách đã chốt.
- Chương 8 mô tả Concept, Entity và Physical Model đã đồng bộ với SRS.

---
# 2. Overall Description

## 2.1 Product Perspective

Hệ thống là một web app quản lý tòa nhà gửi xe, hoạt động như một hệ thống mô phỏng quy trình vận hành bãi xe. Thay vì
tích hợp camera, thẻ vật lý, barrier hoặc cảm biến, toàn bộ dữ liệu như biển số, loại xe, mã thẻ, thao tác vào/ra, thanh
toán sẽ được nhập hoặc xác nhận thủ công trên web.

Hệ thống thay thế một phần quy trình thủ công hiện tại bằng quy trình số hóa: ghi nhận xe vào/ra, kiểm tra chỗ trống,
phân bổ chỗ đỗ, tính phí, thanh toán và theo dõi trạng thái bãi xe.

---

## 2.2 Product Functions

| Function Group                      | Mô tả                                                                                                                                                                |
|-------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Parking Structure Management        | Quản lý Building, Floor, Zone, Slot ở mức cấu hình.                                                                                                                  |
| Driver Account & Vehicle Management | Cho phép tạo tài khoản Driver không cần có xe ban đầu; một tài khoản có thể thêm nhiều xe.                                                                           |
| Vehicle Check-in                    | Tiếp nhận xe vào bãi bằng nhập liệu thủ công.                                                                                                                        |
| Parking Allocation                  | Khi check-in, xe máy được hệ thống gợi ý Zone còn capacity; ô tô Walk-in/Booking chỉ dùng Slot trong Zone `GENERAL`; ô tô thẻ tháng chỉ dùng Slot đã cấp trong Zone `MONTHLY`. |
| Booking Management                  | Booking yêu cầu chọn Building trước, nhập biển số/Vehicle, thanh toán Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành, thời gian đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng; Driver không chọn Zone/Slot. |
| Monthly Subscription Management     | Đăng ký/gia hạn quyền lợi gửi xe tháng gắn với Vehicle, Building, Card `MONTHLY`; xe máy giữ một đơn vị capacity động, ô tô được cấp Slot riêng trong Zone `MONTHLY`. |
| Parking Session Tracking            | Theo dõi lượt gửi xe từ check-in đến check-out.                                                                                                                      |
| Vehicle Check-out                   | Tính phí, xác nhận thanh toán và kết thúc lượt gửi xe.                                                                                                               |
| Payment Management                  | Hỗ trợ tiền mặt và thanh toán online thật qua ngân hàng; không hỗ trợ thanh toán thẻ.                                                                                |
| Fee Calculation                     | Tính phí theo thời gian thực tế sử dụng, loại xe, từng pricing window, base duration, base price, block phát sinh và window cap.                                     |
| Pricing Policy Management           | Cho phép Manager cấu hình bảng giá, khung giờ, block tính phí, cap từng khung giờ, grace period, phí phạt và rule làm tròn.                                          |
| System State Management             | Quản lý trạng thái Building, Floor, Zone, Slot, Vehicle, Card, Parking Session, Booking, Incident, Monthly Subscription và Pricing Policy theo các trạng thái nghiệp vụ đã định nghĩa. |
| Exception Handling                  | Xử lý mất mã, sai biển số, quá hạn, sai khu vực, chưa thanh toán.                                                                                                    |
| Operation Monitoring                | Manager xem trạng thái bãi, doanh thu, lượt xe, tỷ lệ lấp đầy.                                                                                                       |

---

## 2.3 User Classes and Characteristics

| User Class            | Mục tiêu sử dụng                                                              | Đặc điểm                                      |
|-----------------------|-------------------------------------------------------------------------------|-----------------------------------------------|
| Parking Manager       | Quản lý cấu trúc bãi xe, bảng giá, tình trạng vận hành, báo cáo.              | Dùng để giám sát và cấu hình.                 |
| Parking Staff         | Check-in, check-out, thu phí, xử lý ngoại lệ.                                 | Dùng thường xuyên trong vận hành hằng ngày.   |
| Parking User / Driver | Xem thông tin bãi, booking, đăng ký thẻ tháng, theo dõi lượt gửi, thanh toán. | Có thể dùng self-service nếu hệ thống hỗ trợ. |
| System Administrator  | Quản lý tài khoản, phân quyền, cấu hình hệ thống.                             | Dùng để quản trị hệ thống.                    |

---

## 2.4 Operating Environment

Hệ thống chạy trên web browser. Người dùng thao tác qua giao diện web.

Không yêu cầu:

- Camera thật.
- Thẻ vật lý thật.
- Máy quét thẻ.
- Cảm biến slot.
- Barrier tự động.
- Thiết bị IoT.

Có thể triển khai theo hướng:

- Web app nội bộ.
- Web app demo/mô phỏng.
- Web app có thể mở rộng tích hợp hardware trong tương lai, nhưng không thuộc scope hiện tại.

---

## 2.5 Constraints

| Constraint                       | Mô tả                                                                                                                                                                                   |
|----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| No real hardware                 | Tất cả dữ liệu được nhập thủ công hoặc mô phỏng.                                                                                                                                        |
| Manual operation                 | Staff phải nhập/xác nhận biển số, mã thẻ, check-in, check-out.                                                                                                                          |
| Vehicle type limitation          | Phiên bản hiện tại chỉ kích hoạt nghiệp vụ cho xe máy và ô tô.<br/>Hệ thống không hard-code loại xe, nhưng các loại xe khác chưa được phân tích nghiệp vụ chi tiết trong phiên bản này. |
| Motorcycle allocation level      | Xe máy chỉ được phân bổ tới Zone/Area, không phân bổ slot cụ thể.                                                                                                                       |
| Check-in allocation level        | Khi check-in, hệ thống phân bổ vị trí thực tế: xe máy lưu Zone, ô tô lưu Slot.                                                                                                              |
| Motorcycle booking level         | Xe máy booking chỉ chọn Building và Vehicle/biển số; hệ thống giữ một đơn vị general capacity tại Building và chọn Zone khi check-in. |
| Car booking level                | Ô tô booking chỉ chọn Building và Vehicle/biển số; hệ thống giữ general capacity tại Building và chọn Slot trong Zone `GENERAL` khi check-in. |
| Monthly subscription guarantee           | Quyền lợi tháng xe máy giữ capacity động nhưng không giữ Zone/Slot cụ thể; quyền lợi tháng ô tô giữ Slot riêng bằng `monthly_subscription.assigned_slot_id`. |
| Monthly subscription capacity protection | Mỗi Monthly Subscription xe máy `ACTIVE` làm giảm capacity Walk-in/Booking một đơn vị; ô tô Walk-in/Booking chỉ dùng Zone `GENERAL`, ô tô tháng chỉ dùng Zone `MONTHLY`. |
| Pricing model                    | Hệ thống áp dụng mô hình Time Window, Base Price, Increment/Block Pricing và Window Cap.                                                                                                |
| Window cap limitation            | Window Cap chỉ áp dụng cho từng khung giờ riêng biệt, không áp dụng cho toàn bộ phiên gửi xe.                                                                                           |
| 24/7 operation                   | Hệ thống vận hành 24/7 và không reset parking session khi qua ngày mới.                                                                                                                 |
| Dynamic configuration            | Các biến giá, khung giờ, block, grace period, timeout, penalty và rounding phải được cấu hình động, không hardcode vào source code.                                                     |
| Cash rounding                    | Thanh toán tiền mặt có thể làm tròn theo đơn vị cấu hình.                                                                                                                               |
| Online payment rounding          | Thanh toán online không làm tròn, giữ nguyên giá trị chính xác.                                                                                                                         |
| Booking time limit               | Booking chỉ được tạo nếu thời gian đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng so với thời điểm thanh toán cọc thành công.                                                            |
| Booking deposit policy           | Booking yêu cầu thanh toán Deposit Fee trước. Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.                                                                          |
| Booking cancellation policy      | Nếu khách hủy trước thời điểm booking ít nhất 1 tiếng, hệ thống hoàn cọc.                                                                                                               |
| Booking no-show policy           | Nếu khách đến trễ quá 45 phút so với giờ booking, booking bị hủy và khách mất cọc.                                                                                                      |
| Payment method limitation        | Hỗ trợ thanh toán tiền mặt và thanh toán online thật qua ngân hàng; không hỗ trợ thanh toán bằng thẻ.                                                                                   |
| Expandable structure             | Hệ thống phải cho phép mở rộng Building, Floor, Zone, Slot ở mức quản lý dữ liệu nghiệp vụ.                                                                                             |

---

## 2.6 Assumptions and Dependencies

| ID    | Assumption                                                                                                                                          |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| A-001 | Staff là người thao tác chính trong check-in/check-out nếu người dùng không tự thực hiện được.                                                      |
| A-002 | Biển số xe được nhập thủ công, nên hệ thống cần kiểm tra trùng, sai hoặc thiếu biển số.                                                             |
| A-003 | Staff nhập `card_code` của Card vận hành khi check-in/check-out; Card hiện tại là dữ liệu vận hành trên web và có thể mở rộng bằng `nfc_uid` khi tích hợp NFC sau này. |
| A-004 | Xe máy được quản lý theo sức chứa Zone/Area, không theo slot cụ thể.                                                                                |
| A-005 | Ô tô khi gửi thực tế cần được gán slot cụ thể để tránh xung đột vị trí.                                                                             |
| A-006 | Khi check-in, xe máy và ô tô đều được hệ thống gợi ý theo Zone còn chỗ.                                                                             |
| A-007 | Xe máy booking phải chọn Building trước; Driver không chọn Zone/Slot, hệ thống chọn Zone phù hợp khi check-in. |
| A-008 | Ô tô booking phải chọn Building trước; Driver không chọn Zone/Slot, hệ thống chọn Slot trong Zone `GENERAL` khi check-in. |
| A-009 | Booking chỉ hợp lệ sau khi thanh toán cọc thành công.                                                                                               |
| A-010 | Booking phải được đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng tính từ thời điểm thanh toán cọc thành công.                                        |
| A-011 | Booking phải có thời gian dự kiến gửi xe để hệ thống tính tiền cọc.                                                                                 |
| A-012 | Tiền cọc booking bằng giá của block đầu tiên theo bảng giá hiện hành.                                                                               |
| A-013 | Nếu khách hủy booking trước giờ booking ít nhất 1 tiếng, hệ thống hoàn cọc.                                                                         |
| A-014 | Nếu khách đến trễ quá 45 phút so với giờ booking, booking bị hủy và khách mất cọc.                                                                  |
| A-015 | Nếu khách đến sớm hơn giờ booking, hệ thống cho phép check-in sớm nếu còn chỗ phù hợp. Thời gian gửi xe bắt đầu tính từ lúc khách thực tế check-in. |
| A-016 | Nếu khách dùng đúng trong khoảng thời gian đã đặt, khi check-out khách thanh toán phần còn lại sau khi trừ cọc.                                     |
| A-017 | Nếu khách gửi vượt quá thời gian đã đặt, phần vượt được tính phí theo chính sách gửi xe thông thường.                                               |
| A-018 | Thẻ tháng xe máy đảm bảo có chỗ bằng capacity động: mỗi Monthly Subscription `ACTIVE` giảm capacity Walk-in/Booking một đơn vị, không phân Zone/Slot cụ thể. |
| A-019 | Thẻ tháng ô tô được cấp Slot riêng trong Zone `MONTHLY`; quyền giữ Slot nằm ở `monthly_subscription.assigned_slot_id`, không nằm trong `slot_status`. |
| A-020 | Thẻ tháng không giới hạn số lượt vào/ra mỗi ngày trong thời gian còn hiệu lực.                                                                      |
| A-021 | Một Driver Account có thể tồn tại mà chưa có xe. Xe có thể được thêm sau.                                                                           |
| A-022 | Một Driver Account có thể quản lý nhiều xe.                                                                                                         |
| A-023 | Online payment là thanh toán thật qua ngân hàng, không phải mô phỏng và không phải thanh toán thẻ.                                                  |
| A-024 | Phí gửi xe có thể thay đổi theo loại xe, số giờ, khung giờ sáng/tối, qua đêm và nhiều ngày.                                                         |
| A-025 | Hệ thống tính phí theo thời gian thực tế sử dụng.                                                                                                   |
| A-026 | Nếu phiên gửi xe đi qua nhiều pricing window, hệ thống tách phiên theo từng khung giờ và áp dụng bảng giá riêng cho từng khung.                     |
| A-027 | Window Cap chỉ giới hạn phí trong từng pricing window, không giới hạn toàn bộ session.                                                              |
| A-028 | Hệ thống vận hành 24/7 và không reset session khi qua ngày mới.                                                                                     |
| A-029 | Nếu vé tháng hết hạn lúc xe vẫn còn trong bãi, hệ thống downgrade quyền lợi vé tháng và bắt đầu tính phí vãng lai từ thời điểm hết hạn.             |
| A-030 | Booking chưa thanh toán trong thời gian timeout sẽ tự động bị hủy và slot/capacity được trả về trạng thái available.                                |
| A-031 | Nếu khách booking không check-in trong thời gian ân hạn, booking bị hủy và deposit fee không được hoàn trả.                                         |
| A-032 | Nếu khách check-out trễ hơn thời gian booking, phần phát sinh được tính theo block pricing của bảng giá vãng lai.                                   |
| A-033 | Nếu thời gian phát sinh nhỏ hơn hoặc bằng grace period, hệ thống không tính block mới. Nếu vượt grace period, hệ thống tính thành block mới.        |
| A-034 | Thanh toán tiền mặt áp dụng cash rounding nếu có số lẻ, discount hoặc VAT.                                                                          |
| A-035 | Thanh toán online không làm tròn số tiền.                                                                                                           |

---
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
# 4. Business Context

## 4.1 Problem Statement

Hiện tại, nghiệp vụ gửi xe trong tòa nhà nhiều tầng có nhiều điểm dễ sai sót: xe ra/vào liên tục, cần kiểm soát chỗ
trống, cần tính phí, cần xử lý mất vé/mã gửi xe, quá hạn, sai thông tin xe, xe gửi sai khu vực và xe chưa thanh toán.
Nếu quản lý thủ công, bãi xe dễ bị ùn ứ, sai lệch dữ liệu, khó kiểm soát sức chứa và khó đối soát doanh thu.

Trong phiên bản này, vì không có hardware thật, hệ thống cần mô phỏng nghiệp vụ bằng web app nhưng vẫn phải giữ đúng
logic vận hành: xe vào phải có session, xe phải được phân bổ đúng khu vực/slot, booking và thẻ tháng không được làm vượt
sức chứa, xe ra phải được thanh toán và cập nhật trạng thái.

---

## 4.2 Business Goals

| Goal ID | Business Goal                                                                         |
|---------|---------------------------------------------------------------------------------------|
| BG-001  | Chuẩn hóa quy trình xe vào/ra bằng web app.                                           |
| BG-002  | Giảm sai sót khi ghi nhận biển số, mã gửi xe, giờ vào/ra, phí gửi xe.                 |
| BG-003  | Kiểm soát được chỗ trống theo Zone cho xe máy và Slot cho ô tô.                       |
| BG-004  | Hỗ trợ đặt chỗ trước để người dùng chủ động kế hoạch gửi xe.                          |
| BG-005  | Hỗ trợ thẻ tháng nhưng không vượt quá khả năng phục vụ của bãi.                       |
| BG-006  | Hỗ trợ thanh toán tiền mặt và online qua ngân hàng; không hỗ trợ thanh toán bằng thẻ. |
| BG-007  | Cho phép mở rộng mô hình bãi xe khi có thêm Building, Floor, Zone, Slot.              |
| BG-008  | Giúp Manager theo dõi vận hành, doanh thu, lượt xe, tỷ lệ lấp đầy.                    |

---

## 4.3 Success Criteria

| Criteria ID | Success Criteria                                                                                       |
|-------------|--------------------------------------------------------------------------------------------------------|
| SC-001      | Driver có thể tạo tài khoản mà chưa cần thêm xe.                                                       |
| SC-002      | Một Driver Account có thể thêm nhiều xe.                                                               |
| SC-003      | Booking bắt buộc nhập biển số.                                                                         |
| SC-004      | Booking chỉ confirmed sau khi thanh toán cọc thành công.                                               |
| SC-005      | Booking phải được đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng tính từ lúc thanh toán cọc thành công. |
| SC-006      | Xe máy booking phải chọn Building trước; người dùng không chọn Zone/Slot, hệ thống chọn Zone khi check-in. |
| SC-007      | Ô tô booking phải chọn Building trước; người dùng không chọn Zone/Slot, hệ thống chọn Slot trong Zone `GENERAL` khi check-in. |
| SC-008      | Booking yêu cầu thanh toán Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.            |
| SC-009      | Booking được hoàn cọc nếu khách hủy trước giờ booking ít nhất 1 tiếng.                                 |
| SC-010      | Booking bị hủy và mất cọc nếu khách đến trễ quá 45 phút.                                               |
| SC-011      | Thẻ tháng xe máy đảm bảo có chỗ bằng capacity động: mỗi subscription `ACTIVE` giảm capacity Walk-in/Booking một đơn vị. |
| SC-012      | Thẻ tháng ô tô được cấp Slot riêng trong Zone `MONTHLY`; Slot được giữ bằng `monthly_subscription.assigned_slot_id`. |
| SC-013      | Thẻ tháng không giới hạn số lượt vào/ra mỗi ngày.                                                      |
| SC-014      | Hệ thống không cho cấp thêm thẻ tháng nếu vượt khả năng đảm bảo chỗ.                                   |
| SC-015      | Hệ thống hỗ trợ thanh toán tiền mặt và online qua ngân hàng.                                           |
| SC-016      | Hệ thống không hiển thị thanh toán bằng thẻ.                                                           |
| SC-017      | Phí gửi xe được tính theo giờ, loại xe, khung giờ sáng/tối, qua đêm và nhiều ngày.                     |
| SC-018      | Hệ thống không cấp hết chỗ cho walk-in/booking nếu phần còn lại cần giữ cho khách thẻ tháng.           |
| SC-019      | Staff có thể tạo lượt gửi xe bằng nhập thủ công biển số, loại xe, mã gửi xe.                           |
| SC-020      | Xe máy được hệ thống gợi ý Zone/Area còn trống.                                                        |
| SC-021      | Ô tô Walk-in/Booking chỉ được hệ thống gán Slot trong Zone `GENERAL`; ô tô thẻ tháng dùng Slot đã cấp trong Zone `MONTHLY`. |
| SC-022      | Khi check-out, hệ thống tính được phí và ghi nhận thanh toán tiền mặt hoặc online.                     |
| SC-023      | Sau khi xe ra, Zone/Slot được cập nhật lại trạng thái còn trống.                                       |
| SC-024      | Manager có thể xem tình trạng chỗ đỗ, lượt xe và doanh thu.                                            |
| SC-025      | Có thể thêm Building/Floor/Zone/Slot mới mà không làm thay đổi luồng nghiệp vụ chính.                  |
| SC-026 | Hệ thống tính phí theo Time Window, Base Price, Increment/Block Pricing và Window Cap. |
| SC-027 | Nếu session đi qua nhiều khung giờ, hệ thống tách session và tính phí riêng cho từng khung. |
| SC-028 | Window Cap chỉ áp dụng trong từng khung giờ riêng biệt. |
| SC-029 | Session không bị reset khi qua ngày mới. |
| SC-030 | Hệ thống tự động downgrade vé tháng hết hạn nếu xe vẫn còn trong bãi và tính phí vãng lai từ thời điểm hết hạn. |
| SC-031 | Booking chưa thanh toán sau booking payment timeout sẽ tự động bị hủy. |
| SC-032 | Booking no-show sau check-in grace time sẽ bị hủy và không hoàn deposit fee. |
| SC-033 | Thời gian phát sinh nhỏ hơn hoặc bằng grace period không bị tính block mới. |
| SC-034 | Thanh toán tiền mặt được làm tròn theo cash rounding rule. |
| SC-035 | Thanh toán online giữ nguyên giá trị chính xác, không làm tròn. |

---

## 4.4 Current Workflow

### Trước khi có hệ thống

1. Nhân viên tiếp nhận xe.
2. Nhân viên ghi nhận biển số, loại xe, giờ vào bằng cách thủ công.
3. Nhân viên tự kiểm tra hoặc ước lượng còn chỗ hay không.
4. Người gửi xe tự tìm khu vực/slot.
5. Khi xe ra, nhân viên tìm lại thông tin gửi xe.
6. Nhân viên tính phí.
7. Người gửi xe thanh toán.
8. Nhân viên ghi nhận xe đã rời bãi.
9. Manager tổng hợp doanh thu/lượt xe thủ công hoặc bán thủ công.

### Pain Points

| Pain Point                                 | Ảnh hưởng                                     |
|--------------------------------------------|-----------------------------------------------|
| Khó biết còn chỗ chính xác                 | Dễ nhận xe vượt sức chứa.                     |
| Xe máy và ô tô cần logic phân bổ khác nhau | Dễ gửi sai khu vực.                           |
| Không có trạng thái booking rõ ràng        | Có thể trùng chỗ.                             |
| Thẻ tháng nếu cấp quá nhiều                | Không đảm bảo còn chỗ.                        |
| Ghi nhận thủ công                          | Dễ sai biển số, giờ vào, phí.                 |
| Thanh toán khó đối soát                    | Dễ lệch doanh thu.                            |
| Không có hardware thật                     | Cần cơ chế mô phỏng nhưng vẫn đúng nghiệp vụ. |

---

## 4.5 Target Workflow

### Luồng tổng quát sau khi có hệ thống

1. Staff hoặc Driver mở chức năng tương ứng trên web.
2. Hệ thống hiển thị thông tin bãi xe, chỗ trống, giá và quy định.
3. Người dùng nhập dữ liệu: biển số, loại xe, mã thẻ/mã gửi xe, thời gian booking hoặc thông tin thẻ tháng.
4. Hệ thống kiểm tra điều kiện hợp lệ.
5. Hệ thống phân bổ:
    - Xe máy → Zone/Area còn trống.
    - Ô tô vãng lai → Zone còn trống.
    - Ô tô booking → Slot trong Zone `GENERAL` do hệ thống chọn khi check-in.
    - Ô tô thẻ tháng → Slot riêng trong Zone `MONTHLY` đã được cấp qua `monthly_subscription.assigned_slot_id`.
6. Hệ thống tạo booking, monthly registration hoặc parking session.
7. Khi xe ra, Staff tìm session.
8. Hệ thống tính phí.
9. Người dùng thanh toán tiền mặt hoặc online.
10. Hệ thống cập nhật trạng thái session, Zone/Slot và giao dịch.
11. Manager theo dõi trạng thái vận hành qua màn hình quản lý.

### 4.5.1 Booking Workflow

#### Main Flow

1. Driver đăng nhập, tạo booking.
2. Driver chọn Building muốn gửi xe.
3. Driver chọn loại xe: xe máy hoặc ô tô.
4. Driver nhập/chọn biển số xe.
5. Driver nhập thời gian dự kiến vào bãi.
6. Driver nhập thời lượng gửi xe hoặc thời gian dự kiến rời bãi.
7. Hệ thống kiểm tra thời gian booking:
    - Tối thiểu trước 1 tiếng so với thời điểm thanh toán cọc thành công.
    - Tối đa trước 8 tiếng so với thời điểm thanh toán cọc thành công.
8. Hệ thống xác định bảng giá hiện hành theo loại xe và thời điểm booking.
9. Hệ thống tính Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.
10. Hệ thống kiểm tra chỗ khả dụng trong Building đã chọn:
    - Xe máy: Driver không chọn Zone/Slot; hệ thống kiểm tra general capacity của Building sau khi trừ Monthly Subscription xe máy active.
    - Ô tô: Driver không chọn Zone/Slot; hệ thống kiểm tra Slot khả dụng trong các Zone `GENERAL` của Building.
11. Hệ thống tạo yêu cầu thanh toán cọc.
12. Driver thanh toán cọc qua ngân hàng.
13. Nếu thanh toán cọc thành công, hệ thống xác nhận booking và chuyển trạng thái booking sang CONFIRMED.
14. Booking chỉ giữ capacity ở mức Building; vị trí thực tế được hệ thống gán khi check-in và lưu ở Parking Session.
15. Khi Driver đến đúng thời gian hợp lệ, booking được chuyển thành parking session.

#### Exception Flow

| Trường hợp                                            | Xử lý                                                           |
|-------------------------------------------------------|-----------------------------------------------------------------|
| Thiếu Building                                        | Không cho tạo booking.                                          |
| Thiếu biển số                                         | Không cho tạo booking.                                          |
| Không nhập thời lượng gửi xe hoặc giờ rời bãi dự kiến | Không cho tạo booking vì hệ thống không đủ dữ liệu để tính cọc. |
| Thời gian booking nhỏ hơn 1 tiếng từ lúc thanh toán   | Từ chối booking.                                                |
| Thời gian booking lớn hơn 8 tiếng từ lúc thanh toán   | Từ chối booking.                                                |
| Thanh toán cọc thất bại                               | Booking không được xác nhận.                                    |
| Xe máy không còn capacity đảm bảo                     | Từ chối booking.                                                |
| Ô tô không còn Slot phù hợp                           | Từ chối booking.                                                |
| Khách hủy trước giờ booking ít nhất 1 tiếng           | Hủy booking và hoàn cọc.                                        |
| Khách hủy trễ hơn thời hạn được hoàn cọc              | Hủy booking và không hoàn cọc.                                  |
| Khách đến trễ quá 45 phút                             | Hủy booking/parking reservation và mất cọc.                     |
| Biển số đã có booking active trùng thời gian          | Cảnh báo hoặc từ chối tạo booking mới.                          |
| Quá thời gian thanh toán booking | Hệ thống tự động hủy booking và trả slot/capacity về trạng thái available. |
| Khách không check-in trong thời gian ân hạn | Booking bị hủy tự động, deposit fee không được hoàn trả. |

### 4.5.2 Monthly Subscription Workflow

#### Main Flow

1. Driver mở chức năng đăng ký Monthly Subscription/thẻ tháng.
2. Driver chọn hoặc thêm xe vào tài khoản.
3. Hệ thống kiểm tra biển số xe và loại xe.
4. Hệ thống kiểm tra thẻ tháng hiện tại của xe.
5. Hệ thống kiểm tra khả năng đảm bảo chỗ theo loại xe:
    - Xe máy: kiểm tra capacity động; mỗi Monthly Subscription xe máy `ACTIVE` giữ một đơn vị và làm giảm capacity Walk-in/Booking.
    - Ô tô: kiểm tra Slot còn có thể cấp riêng trong Zone `MONTHLY` của Building đã chọn.
6. Nếu còn khả năng đảm bảo chỗ, hệ thống tạo hồ sơ `monthly_subscription` ở trạng thái `PENDING` và tạo yêu cầu thanh toán.
7. Driver thanh toán phí thẻ tháng.
8. Khi thanh toán thành công, hệ thống kích hoạt Monthly Subscription, thiết lập thời gian hiệu lực, gán Card `MONTHLY`, và giữ capacity/slot tương ứng.
9. Khi xe máy có Monthly Subscription vào bãi, Driver dùng Card `MONTHLY`; hệ thống kiểm tra subscription hợp lệ, dùng capacity tháng đã giữ và gợi ý Zone còn chỗ.
10. Khi ô tô có Monthly Subscription vào bãi, Driver dùng Card `MONTHLY`; hệ thống dùng Slot riêng trong Zone `MONTHLY` đã được cấp cho xe đó.
11. Driver có thể ra/vào nhiều lượt trong ngày nếu thẻ còn hiệu lực và không có session đang mở cùng lúc.
12. Hệ thống lưu quyền lợi tháng theo Vehicle, Building, Card `MONTHLY` và Slot nếu là ô tô; Card type không tự cấp quyền lợi nếu không có Monthly Subscription hợp lệ.
13. Hệ thống ghi nhận chu kỳ hiệu lực của vé tháng theo valid duration.
14. Nếu vé tháng được gia hạn thành công trước hoặc sau khi hết hạn, quyền lợi vé tháng được kích hoạt lại ngay sau khi thanh toán thành công.

#### Downgrade Flow

1. Hệ thống quét trạng thái vé tháng vào 00:00 mỗi ngày.
2. Nếu vé tháng đã hết hạn và xe vẫn còn trong bãi, hệ thống chuyển quyền lợi vé tháng sang trạng thái downgraded/transient.
3. Từ thời điểm hết hạn, hệ thống bắt đầu tính phí theo bảng giá vãng lai.
4. Khi xe check-out, khách thanh toán phần phí phát sinh theo bảng giá vãng lai.

#### Exception Flow

| Trường hợp                                | Xử lý                                           |
|-------------------------------------------|-------------------------------------------------|
| Xe chưa được thêm vào tài khoản           | Yêu cầu thêm xe hoặc nhập biển số.              |
| Xe đã có thẻ tháng còn hiệu lực           | Không cho tạo thẻ tháng trùng thời gian.        |
| Hết capacity đảm bảo cho xe máy thẻ tháng | Từ chối đăng ký thẻ tháng xe máy.               |
| Không còn Slot để cấp cho ô tô thẻ tháng  | Từ chối đăng ký thẻ tháng ô tô.                 |
| Thanh toán thất bại                       | Thẻ tháng không được kích hoạt.                 |
| Thẻ tháng hết hạn                         | Không áp dụng quyền lợi thẻ tháng khi check-in. |
| Vé tháng hết hạn khi xe vẫn còn trong bãi | Hệ thống downgrade vé tháng và tính phí vãng lai từ thời điểm hết hạn. |
| Muốn dùng cùng một vé tháng cho xe khác | Hệ thống từ chối vì mỗi vé tháng chỉ áp dụng cho một xe đã đăng ký. |

### 4.5.3 Check-in Workflow

#### Main Flow

1. Staff mở màn hình Check-in.
2. Staff nhập biển số hoặc mã booking, sau đó nhập `card_code` của Card vận hành được cấp cho lượt gửi hiện tại.
3. Hệ thống xác định loại xe và trạng thái liên quan:
    - Khách vãng lai.
    - Khách có booking.
    - Khách có thẻ tháng.
4. Hệ thống kiểm tra xe có session đang mở hay không.
5. Hệ thống kiểm tra điều kiện chỗ đỗ:
    - Xe máy vãng lai: tìm Zone/Area còn sức chứa.
    - Ô tô vãng lai: tìm Slot còn trống trong Zone `GENERAL`.
    - Xe máy thẻ tháng: đảm bảo có chỗ, không phân theo Zone/Slot cố định.
    - Ô tô thẻ tháng: kiểm tra Slot riêng trong Zone `MONTHLY` đã được cấp cho xe.
    - Xe máy booking: kiểm tra booking còn hiệu lực và còn trong thời gian cho phép.
    - Ô tô booking: kiểm tra booking còn hiệu lực và general capacity đã giữ ở Building.
6. Hệ thống gợi ý chỗ đỗ:
    - Xe máy: gợi ý Zone/Area.
    - Ô tô Walk-in/Booking: gán Slot trong Zone `GENERAL`.
    - Ô tô thẻ tháng: hiển thị Slot riêng trong Zone `MONTHLY`.
7. Staff xác nhận check-in.
8. Hệ thống tạo parking session có `card_id`; nếu áp dụng quyền lợi tháng thì lưu thêm `monthly_subscription_id`, nếu từ booking thì lưu `booking_id`.
9. Với Card `NORMAL`, hệ thống chuyển Card sang `ASSIGNED`; với Card `MONTHLY`, Card giữ trạng thái `ASSIGNED`. Slot status chỉ phản ánh trạng thái vật lý của Slot.

#### Exception Flow

| Trường hợp                           | Xử lý                                                                                             |
|--------------------------------------|---------------------------------------------------------------------------------------------------|
| Biển số trống                        | Yêu cầu nhập biển số.                                                                             |
| Xe đã có session đang mở             | Không cho tạo session mới.                                                                        |
| Booking chưa thanh toán cọc          | Không cho check-in theo booking.                                                                  |
| Booking đã bị hủy do trễ quá 45 phút | Không áp dụng booking, khách mất cọc và chỉ được xử lý như khách mới nếu còn chỗ.                 |
| Khách đến sớm hơn giờ booking        | Cho phép check-in sớm nếu còn chỗ phù hợp; thời gian gửi xe bắt đầu tính từ lúc check-in thực tế. |
| Hết chỗ phù hợp                      | Từ chối check-in.                                                                                 |
| Zone/Slot bị khóa hoặc bảo trì       | Không phân bổ.                                                                                    |

### 4.5.4 Check-out & Payment Workflow

#### Main Flow

1. Staff mở màn hình Check-out.
2. Staff nhập `card_code` hoặc thông tin thay thế khi xử lý ngoại lệ.
3. Hệ thống tìm Card và parking session đang mở theo `card_id`.
4. Hệ thống tính phí dựa trên:
   - Loại xe.
   - Thời gian thực tế sử dụng.
   - Pricing Window tương ứng.
   - Base Duration.
   - Base Price.
   - Increment Block.
   - Increment Price.
   - Window Cap của từng khung giờ.
   - Grace Period.
   - Chính sách vé tháng nếu có.
   - Deposit Fee đã thanh toán nếu session đi từ booking.
   - Phí phạt hoặc phụ phí nếu có.
5. Nếu session đi qua nhiều pricing window, hệ thống tách session thành nhiều đoạn thời gian và tính phí riêng cho từng đoạn.
6. Nếu có phần thời gian phát sinh nhỏ hơn hoặc bằng grace period, hệ thống không tính block mới.
7. Nếu thanh toán tiền mặt, hệ thống áp dụng cash rounding rule nếu cần.
8. Nếu thanh toán online, hệ thống giữ nguyên số tiền chính xác.

#### Exception Flow

| Trường hợp                 | Xử lý                                      |
|----------------------------|--------------------------------------------|
| Không tìm thấy session     | Báo không có lượt gửi đang mở.             |
| Thanh toán online thất bại | Cho thanh toán lại hoặc đổi sang tiền mặt. |
| Chưa thanh toán            | Không cho hoàn tất check-out.              |
| Sai biển số                | Staff xử lý ngoại lệ trước khi check-out.  |
| Có phí phát sinh           | Cộng vào tổng phí trước khi thanh toán.    |

---
# 5. Functional Requirements

## Feature List

| Feature ID | Feature Name                        | Priority | Description                                                                                |
|------------|-------------------------------------|----------|--------------------------------------------------------------------------------------------|
| F-001      | Parking Structure Management        | Must     | Quản lý Building, Floor, Zone, Slot để hệ thống có thể mở rộng.                            |
| F-002      | Driver Account & Vehicle Management | Must     | Cho phép tạo tài khoản Driver không cần có xe ban đầu; một tài khoản có thể thêm nhiều xe. |
| F-003      | Vehicle Check-in                    | Must     | Tạo lượt gửi xe bằng nhập liệu thủ công trên web.                                          |
| F-004      | Parking Allocation                  | Must     | Xe máy được gợi ý Zone/Area; ô tô Walk-in/Booking dùng Slot trong Zone `GENERAL`; ô tô thẻ tháng dùng Slot đã cấp trong Zone `MONTHLY`. |
| F-005      | Booking Management                  | Must     | Booking yêu cầu chọn Building trước, nhập biển số, đặt cọc bằng giá block đầu tiên, thời gian đặt trước tối thiểu 1 tiếng từ lúc thanh toán. |
| F-006      | Monthly Subscription Management     | Must     | Đăng ký/gia hạn quyền lợi gửi xe tháng gắn với Vehicle, đảm bảo luôn còn chỗ bằng capacity/slot riêng. |
| F-007      | Parking Session Tracking            | Must     | Theo dõi lượt gửi xe từ check-in đến check-out.                                            |
| F-008      | Vehicle Check-out                   | Must     | Tính phí, xác nhận thanh toán và kết thúc lượt gửi xe.                                     |
| F-009      | Payment Management                  | Must     | Hỗ trợ tiền mặt và thanh toán online thật qua ngân hàng; không hỗ trợ thanh toán thẻ.      |
| F-010      | Fee Calculation                     | Must     | Tính phí theo giờ, loại xe, khung giờ, qua đêm và nhiều ngày.                              |
| F-011      | Exception Handling                  | Should   | Xử lý mất mã, sai biển số, quá hạn, gửi sai khu vực, chưa thanh toán.                      |
| F-012      | Operation Monitoring                | Should   | Manager xem trạng thái bãi, doanh thu, lượt xe, tỷ lệ lấp đầy.                             |

---

## FR-001: Parking Structure Management

### Description

Hệ thống cho phép Manager cấu hình cấu trúc bãi xe để phục vụ vận hành và mở rộng sau này.

### Actors

- Parking Manager
- System Administrator

### Preconditions

- Actor đã đăng nhập.
- Actor có quyền quản lý cấu trúc bãi xe.

### Trigger

- Actor mở màn hình quản lý cấu trúc bãi xe.

### Main Flow

1. Actor chọn chức năng quản lý cấu trúc bãi xe.
2. Hệ thống hiển thị danh sách Building, Floor, Zone, Slot hiện có.
3. Actor thêm hoặc cập nhật cấu trúc.
4. Hệ thống kiểm tra dữ liệu hợp lệ.
5. Hệ thống lưu cấu hình.
6. Hệ thống cập nhật dữ liệu để phục vụ phân bổ chỗ đỗ.

### Alternative Flow

- Actor tạm khóa Zone/Slot để bảo trì.
- Actor mở lại Zone/Slot sau khi bảo trì.
- Actor thay đổi Zone dành cho loại xe cụ thể.

### Exception Flow

- Nếu Zone/Slot đang có xe hoặc booking, hệ thống không cho xóa trực tiếp.
- Nếu cấu hình làm vượt hoặc sai sức chứa, hệ thống báo lỗi.
- Nếu actor không có quyền, hệ thống từ chối thao tác.

### Business Rules

- BR-001
- BR-002
- BR-003
- BR-004
- BR-005
- BR-006

### Postconditions

- Hệ thống lưu cấu hình.
- Hệ thống cập nhật dữ liệu để phục vụ phân bổ chỗ đỗ.

### Acceptance Criteria

- Given Manager có quyền quản lý cấu trúc  
  When Manager thêm Building/Floor/Zone/Slot hợp lệ  
  Then hệ thống lưu cấu trúc và cho phép dùng trong phân bổ chỗ.

- Given Zone/Slot đang bảo trì  
  When Staff check-in hoặc Driver booking  
  Then hệ thống không phân bổ Zone/Slot đó.

---

## FR-002: Driver Account & Vehicle Management

### Description

Hệ thống cho phép tạo tài khoản Driver trước, không bắt buộc phải có xe ngay lúc tạo tài khoản. Driver có thể thêm một
hoặc nhiều xe sau này.

### Actors

- Driver
- Parking Staff
- System

### Preconditions

- Driver hoặc Staff có quyền tạo/cập nhật thông tin tài khoản.
- Biển số xe được cung cấp khi thêm xe.

### Trigger

- Driver đăng ký tài khoản.
- Driver thêm xe vào tài khoản.
- Staff tạo hoặc cập nhật tài khoản hộ Driver.

### Main Flow

1. Actor mở màn hình quản lý tài khoản Driver.
2. Actor tạo tài khoản Driver với thông tin cơ bản.
3. Hệ thống cho phép lưu tài khoản dù chưa có xe.
4. Khi cần, Actor chọn Add Vehicle.
5. Actor nhập biển số, loại xe.
6. Hệ thống kiểm tra dữ liệu xe.
7. Hệ thống lưu xe vào tài khoản Driver.

### Alternative Flow

- Staff thêm xe hộ Driver.
- Driver thêm nhiều xe vào cùng một tài khoản.

### Exception Flow

| Trường hợp                                  | Xử lý                                          |
|---------------------------------------------|------------------------------------------------|
| Biển số trống khi thêm xe                   | Hệ thống yêu cầu nhập biển số.                 |
| Loại xe không được hỗ trợ                   | Hệ thống từ chối.                              |
| Biển số đã tồn tại theo rule kiểm tra trùng | Hệ thống cảnh báo hoặc từ chối tùy chính sách. |

### Business Rules

- BR-ACC-001
- BR-ACC-002
- BR-ACC-003
- BR-ACC-004

### Postconditions

- Driver Account được tạo.
- Xe được liên kết với Driver Account nếu Actor thêm xe.

### Acceptance Criteria

- Given Driver chưa có xe  
  When Driver tạo tài khoản  
  Then hệ thống vẫn cho tạo tài khoản thành công.

- Given Driver đã có tài khoản  
  When Driver thêm nhiều xe hợp lệ  
  Then hệ thống lưu các xe vào cùng tài khoản.

---

## FR-003: Vehicle Check-in

### Description

Hệ thống cho phép Staff tạo lượt gửi xe khi xe vào bãi bằng cách nhập dữ liệu thủ công trên web.

### Actors

- Parking Staff
- System

### Preconditions

- Staff đã đăng nhập.
- Bãi xe đang hoạt động.
- Có ít nhất một Zone/Slot còn khả dụng cho loại xe tương ứng.

### Trigger

- Xe đến bãi và cần check-in.

### Main Flow

1. Staff mở màn hình Check-in.
2. Staff nhập biển số, mã booking hoặc `card_code`; Walk-in/Booking dùng Card `NORMAL`, Monthly Subscription dùng Card `MONTHLY` đã gán với subscription.
3. Hệ thống xác định loại xe và trạng thái liên quan:
    - Khách vãng lai.
    - Khách có booking.
    - Khách có thẻ tháng.
4. Hệ thống kiểm tra xe có session đang mở hay không.
5. Hệ thống kiểm tra điều kiện chỗ đỗ:
    - Xe máy: tìm Zone/Area còn sức chứa.
    - Ô tô Walk-in/Booking: tìm Slot còn trống trong Zone `GENERAL`.
    - Xe máy thẻ tháng: dùng capacity tháng đã giữ tại Building.
    - Ô tô thẻ tháng: dùng Slot riêng trong Zone `MONTHLY`.
    - Khách booking: kiểm tra booking còn hiệu lực.
6. Hệ thống gợi ý Zone/Slot phù hợp.
7. Staff xác nhận check-in.
8. Hệ thống tạo parking session có `card_id`; nếu áp dụng quyền lợi tháng thì lưu `monthly_subscription_id`.
9. Hệ thống chuyển Card `NORMAL` sang `ASSIGNED`; Card `MONTHLY` giữ trạng thái `ASSIGNED`; hệ thống cập nhật Zone/Slot theo trạng thái vật lý.

### Alternative Flow

- Nếu xe có booking hợp lệ, hệ thống lấy thông tin booking để tạo session.
- Nếu xe có thẻ tháng hợp lệ, hệ thống áp dụng quyền lợi Monthly Subscription thông qua Card `MONTHLY` đã gán với subscription.
- Nếu Staff nhập sai thông tin trước khi xác nhận, Staff có thể sửa lại.

### Exception Flow

| Trường hợp                           | Xử lý                                                   |
|--------------------------------------|---------------------------------------------------------|
| Biển số trống                        | Yêu cầu nhập biển số.                                   |
| Xe đã có session đang mở             | Không cho tạo session mới.                              |
| Booking chưa thanh toán cọc          | Không cho check-in theo booking.                        |
| Booking đã bị hủy do trễ quá 45 phút | Không áp dụng booking, xử lý như khách mới nếu còn chỗ. |
| Hết chỗ phù hợp                      | Từ chối check-in.                                       |
| Card không khả dụng                  | Từ chối check-in.                                       |
| Zone/Slot bị khóa hoặc bảo trì       | Không phân bổ.                                          |

### Business Rules

- BR-HW-001
- BR-HW-002
- BR-HW-003
- BR-ALLOC-001
- BR-ALLOC-002
- BR-ALLOC-003
- BR-ALLOC-004
- BR-ALLOC-005
- BR-BOOK-008
- BR-MONTH-001
- BR-MONTH-002
- BR-MONTH-003
- BR-028
- BR-030

### Postconditions

- Hệ thống tạo parking session.
- Parking session có `card_id` bắt buộc trong scope hiện tại.
- Hệ thống cập nhật trạng thái chỗ đỗ.

### Acceptance Criteria

- Given Staff nhập biển số và loại xe hợp lệ  
  When hệ thống còn chỗ phù hợp  
  Then hệ thống tạo parking session thành công.

- Given xe đang có session chưa hoàn tất  
  When Staff check-in cùng biển số  
  Then hệ thống báo xe đang ở trong bãi.

---

## FR-004: Parking Allocation

### Description

Hệ thống phân bổ chỗ đỗ theo loại xe và theo trạng thái khách vãng lai, booking hoặc thẻ tháng.

### Actors

- Parking Staff
- Driver
- System

### Preconditions

- Cấu trúc Building/Floor/Zone/Slot đã được cấu hình.
- Zone/Slot có trạng thái hợp lệ.
- Loại xe đã được xác định.

### Trigger

- Check-in xe vãng lai.
- Check-in xe có booking.
- Check-in xe có thẻ tháng.

### Main Flow

1. Hệ thống nhận thông tin loại xe và trạng thái khách.
2. Hệ thống xác định nhóm phân bổ:
    - Walk-in.
    - Booking.
    - Monthly Subscription.
3. Hệ thống kiểm tra Zone/Slot khả dụng.
4. Hệ thống loại bỏ Zone/Slot bảo trì, tạm khóa hoặc không phù hợp.
5. Nếu là xe máy, hệ thống gợi ý Zone/Area còn sức chứa.
6. Nếu là ô tô Walk-in/Booking, hệ thống gán Slot còn trống trong Zone `GENERAL`.
7. Nếu là ô tô thẻ tháng, hệ thống dùng Slot riêng đã cấp trong Zone `MONTHLY`.
8. Nếu là xe máy thẻ tháng, hệ thống dùng capacity tháng đã giữ và gợi ý Zone còn chỗ khi check-in.
9. Walk-in/Booking không được dùng capacity xe máy đã giữ cho Monthly Subscription hoặc Slot trong Zone `MONTHLY`.
10. Staff xác nhận phân bổ.
11. Hệ thống cập nhật trạng thái.

### Alternative Flow

- Staff có thể điều chỉnh phân bổ nếu vẫn hợp lệ với nhóm sử dụng: ô tô Walk-in/Booking chỉ `GENERAL`, ô tô thẻ tháng chỉ Slot đã gán trong `MONTHLY`.
- Hệ thống có thể gợi ý nhiều lựa chọn nếu có nhiều chỗ phù hợp.

### Exception Flow

| Trường hợp                              | Xử lý                          |
|-----------------------------------------|--------------------------------|
| Không còn Zone cho xe máy               | Từ chối check-in.              |
| Không còn Zone/Slot phù hợp cho ô tô    | Từ chối check-in.              |
| Phần chỗ còn lại phải giữ cho thẻ tháng | Không cấp cho walk-in/booking. |
| Zone/Slot bảo trì                       | Không phân bổ.                 |

### Business Rules

- BR-ALLOC-001
- BR-ALLOC-002
- BR-ALLOC-003
- BR-ALLOC-004
- BR-ALLOC-005

### Postconditions

- Xe được gán Zone/Slot phù hợp.
- Capacity được cập nhật theo session/booking/subscription; Slot status chỉ phản ánh trạng thái vật lý hoặc vận hành của Slot.

### Acceptance Criteria

- Given xe máy check-in  
  When còn Zone phù hợp  
  Then hệ thống gợi ý Zone/Area còn sức chứa.

- Given ô tô check-in  
  When còn Slot phù hợp  
  Then hệ thống gán Slot cụ thể.

- Given khách thẻ tháng check-in  
  When Monthly Subscription còn hiệu lực  
  Then xe máy dùng capacity tháng đã giữ, ô tô dùng Slot `MONTHLY` đã gán.

---

## FR-005: Booking Management

### Description

Hệ thống cho phép Driver đặt trước chỗ gửi xe.
Booking yêu cầu chọn Building trước, nhập biển số, thời gian gửi dự kiến và thanh toán Deposit Fee.
Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.
Booking phải được tạo tối thiểu 1 tiếng và tối đa 8 tiếng trước thời điểm vào bãi tính từ lúc thanh toán cọc thành công.
Driver không chọn Zone/Slot khi Booking. Booking chỉ giữ capacity ở mức Building; hệ thống gán Zone/Slot thực tế khi check-in và lưu vào Parking Session.

### Actors

- Driver
- Parking Staff
- System
- Bank Payment Gateway

### Preconditions

- Driver có tài khoản hoặc được Staff tạo booking hộ.
- Driver chọn Building trước khi tạo booking.
- Driver cung cấp biển số xe.
- Driver cung cấp thời gian dự kiến vào bãi.
- Driver cung cấp thời lượng gửi xe hoặc thời gian dự kiến rời bãi.
- Hệ thống còn chỗ phù hợp trong Building đã chọn:
    - Xe máy: còn general capacity sau khi trừ Monthly Subscription xe máy `ACTIVE`.
    - Ô tô: còn Slot khả dụng trong Zone `GENERAL`.
- Người dùng thanh toán cọc thành công.

### Trigger

- Driver muốn đặt trước chỗ gửi xe.

### Main Flow

1. Driver mở chức năng Booking.
2. Driver chọn Building muốn gửi xe.
3. Driver chọn hoặc nhập biển số xe.
4. Driver chọn loại xe.
5. Driver nhập thời gian dự kiến vào bãi.
6. Driver nhập thời lượng gửi xe hoặc thời gian dự kiến rời bãi.
7. Hệ thống kiểm tra thời gian đặt trước:
    - Không nhỏ hơn 1 tiếng.
    - Không lớn hơn 8 tiếng.
8. Hệ thống xác định bảng giá hiện hành theo loại xe và thời điểm booking.
9. Hệ thống tính Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.
10. Hệ thống kiểm tra general capacity trong Building đã chọn:
    - Xe máy: tính capacity còn lại sau khi trừ active motorcycle Monthly Subscriptions, active general motorcycle sessions và confirmed motorcycle bookings.
    - Ô tô: chỉ tính Slot khả dụng trong các Zone `GENERAL`, trừ active general car sessions và confirmed car bookings.
11. Hệ thống tạo yêu cầu đặt cọc.
12. Driver thanh toán cọc qua ngân hàng.
13. Bank Payment Gateway trả kết quả thanh toán.
14. Nếu thanh toán thành công, hệ thống xác nhận booking.
15. Khi Driver đến bãi trong thời gian hợp lệ, hệ thống chuyển booking thành parking session.
16. Booking confirmed giữ capacity ở mức Building; không dùng `slot_status` để giữ booking và không lưu Zone/Slot do user chọn.
17. Nếu khách không thanh toán trong thời gian booking payment timeout, hệ thống tự động hủy booking.

### Alternative Flow

- Staff tạo booking hộ Driver.
- Driver hủy booking trước thời gian quy định nếu chính sách cho phép.
- Driver đổi thời gian booking nếu còn general capacity phù hợp trong Building đã chọn.

### Exception Flow

| Trường hợp                                                     | Xử lý                                       |
|----------------------------------------------------------------|---------------------------------------------|
| Không chọn Building                                            | Không cho tạo booking.                      |
| Không nhập biển số                                             | Không cho tạo booking.                      |
| Thời gian booking không đủ tối thiểu 1 tiếng từ lúc thanh toán | Từ chối booking.                            |
| Thanh toán cọc thất bại                                        | Booking không được confirmed.               |
| Không còn chỗ phù hợp                                          | Từ chối booking.                            |
| Đến trễ quá 45 phút                                            | Hủy booking/parking reservation và mất cọc. |
| Booking trùng biển số trong cùng thời gian                     | Cảnh báo hoặc từ chối.                      |

### Business Rules

- BR-BOOK-001
- BR-BOOK-002
- BR-BOOK-003
- BR-BOOK-004
- BR-BOOK-005
- BR-BOOK-006
- BR-BOOK-007
- BR-BOOK-008
- BR-BOOK-009
- BR-BOOK-010

### Postconditions

- Booking được tạo ở trạng thái Confirmed nếu cọc thành công.
- General capacity của Building được giữ theo rule; actual Zone/Slot chỉ được lưu khi check-in.
- Booking có thể chuyển thành Parking Session khi check-in hợp lệ.

### Acceptance Criteria

- Given Driver nhập biển số, thời gian vào bãi và thời lượng gửi xe hợp lệ  
  When Driver thanh toán Deposit Fee bằng giá block đầu tiên thành công  
  Then hệ thống tạo booking confirmed.

- Given booking được xác nhận  
  When thanh toán Deposit Fee thành công  
  Then hệ thống giữ general capacity ở Building và không đổi `slot_status`.

- Given Driver chưa thanh toán booking sau booking payment timeout  
  When hệ thống kiểm tra trạng thái booking  
  Then booking bị hủy và slot/capacity được trả về available.

- Given Driver không check-in trong check-in grace time  
  When hệ thống kiểm tra booking  
  Then booking bị hủy và Deposit Fee không được hoàn trả.

- Given Driver đặt booking dưới 1 tiếng từ thời điểm thanh toán  
  When Driver xác nhận booking  
  Then hệ thống từ chối.

- Given Driver đặt booking quá 8 tiếng từ thời điểm thanh toán  
  When Driver xác nhận booking  
  Then hệ thống từ chối.

- Given Driver booking xe máy  
  When còn capacity phù hợp  
  Then hệ thống xác nhận booking nhưng không gán Zone/Slot cụ thể.

- Given Driver booking ô tô  
  When còn Slot trong Zone `GENERAL` của Building  
  Then hệ thống xác nhận booking ở mức Building và gán Slot thực tế khi check-in.

- Given Driver hủy booking trước giờ booking ít nhất 1 tiếng  
  When hệ thống xử lý hủy  
  Then booking bị hủy và cọc được hoàn.

- Given Driver đến trễ quá 45 phút  
  When hệ thống kiểm tra booking  
  Then booking bị hủy và cọc không hoàn lại.

- Given Driver đến sớm hơn giờ booking  
  When còn chỗ phù hợp  
  Then hệ thống cho check-in sớm và bắt đầu tính giờ từ thời điểm check-in thực tế.

---

## FR-006: Monthly Subscription Management

### Description

Hệ thống cho phép Driver đăng ký/gia hạn Monthly Subscription/thẻ tháng. Monthly Subscription là hồ sơ đăng ký và quyền lợi gửi xe định kỳ gắn với một Vehicle, một Building và một Card `MONTHLY`.
Xe máy được đảm bảo có chỗ bằng capacity động nhưng không phân theo Zone/Slot cụ thể; ô tô được cấp Slot riêng trong Zone `MONTHLY` trong thời hạn quyền lợi.
Quyền lợi tháng áp dụng cho Vehicle đăng ký cố định, được thanh toán trả trước theo chu kỳ, có thời hạn hiệu lực và được xác định bởi Monthly Subscription hợp lệ, không chỉ bởi `card_type`.

### Actors

- Driver
- Parking Staff
- Parking Manager
- System
- Bank Payment Gateway

### Preconditions

- Driver có tài khoản.
- Driver đã có hoặc nhập thông tin xe.
- Hệ thống còn capacity động cho xe máy tháng hoặc Slot trong Zone `MONTHLY` cho ô tô tháng.
- Thanh toán phí thẻ tháng thành công.

### Trigger

- Driver muốn đăng ký hoặc gia hạn thẻ tháng.

### Main Flow

1. Driver hoặc Staff mở chức năng Monthly Subscription.
2. Driver chọn hoặc thêm xe.
3. Hệ thống kiểm tra biển số và loại xe.
4. Hệ thống kiểm tra xe có thẻ tháng còn hiệu lực hay không.
5. Hệ thống kiểm tra khả năng đảm bảo chỗ:
    - Xe máy: kiểm tra capacity động còn lại tại Building.
    - Ô tô: kiểm tra Slot còn có thể cấp riêng trong Zone `MONTHLY`.
6. Nếu còn khả năng đảm bảo, hệ thống tạo `monthly_subscription` ở trạng thái `PENDING` và tạo yêu cầu thanh toán.
7. Driver thanh toán phí thẻ tháng.
8. Hệ thống nhận kết quả thanh toán.
9. Nếu thanh toán thành công, hệ thống kích hoạt Monthly Subscription, thiết lập `activated_at`, `expired_at`, gán Card `MONTHLY`, và gán `assigned_slot_id` nếu là ô tô.
10. Khi xe vào bãi:
- Xe máy thẻ tháng được gợi ý Zone còn chỗ.
- Ô tô thẻ tháng được điều hướng tới Slot riêng đã cấp.
11. Hệ thống lưu thời hạn hiệu lực của vé tháng.
12. Hệ thống xác nhận mỗi vé tháng chỉ liên kết với một xe đã đăng ký.
13. Hệ thống hỗ trợ gia hạn vé tháng; sau khi gia hạn thành công, quyền lợi vé tháng được kích hoạt lại ngay lập tức.
14. Hệ thống gán một Card `MONTHLY` cho Monthly Subscription; Card này được Driver giữ trong thời hạn quyền lợi.

### Alternative Flow

- Staff đăng ký thẻ tháng hộ Driver.
- Driver gia hạn thẻ tháng trước khi hết hạn.
- Manager điều chỉnh cấu hình Zone/capacity; hệ thống không dùng counter quota rời làm nguồn sự thật nếu có thể tính từ Zone, Slot, session, booking và subscription.
- Nếu vé tháng hết hạn và xe vẫn còn trong bãi, hệ thống downgrade quyền lợi vé tháng và tính phí vãng lai từ thời điểm hết hạn.

### Exception Flow

| Trường hợp                      | Xử lý                                 |
|---------------------------------|---------------------------------------|
| Xe chưa được thêm               | Yêu cầu thêm xe hoặc nhập biển số.    |
| Xe đã có thẻ tháng còn hiệu lực | Không cho tạo trùng.                  |
| Hết capacity xe máy tháng hoặc hết Slot `MONTHLY` cho ô tô tháng | Từ chối đăng ký/gia hạn. |
| Thanh toán thất bại             | Không kích hoạt thẻ tháng.            |
| Thẻ tháng hết hạn               | Không áp dụng quyền lợi khi check-in. |

### Business Rules

- BR-MONTH-001
- BR-MONTH-002
- BR-MONTH-003
- BR-MONTH-004
- BR-MONTH-005
- BR-MONTH-006
- BR-MONTH-007
- BR-MONTH-008
- BR-MONTH-009
- BR-MONTH-010
- BR-MONTH-011
- BR-MONTH-012
- BR-MONTH-013
- BR-MONTH-014
- BR-MONTH-015
- BR-MONTH-016

### Postconditions

- Thẻ tháng được kích hoạt nếu thanh toán thành công.
- Capacity động xe máy hoặc Slot tháng ô tô được xác định từ Monthly Subscription active và `assigned_slot_id`.
- Mỗi thẻ tháng chỉ liên kết với một xe đã đăng ký.
- Driver có thể gửi xe nhiều lượt/ngày trong thời gian thẻ còn hiệu lực.

### Acceptance Criteria

- Given Driver đăng ký thẻ tháng xe máy  
  When còn capacity đảm bảo  
  Then thẻ tháng được kích hoạt sau khi thanh toán thành công.

- Given Driver đăng ký thẻ tháng ô tô  
  When còn Slot phù hợp  
  Then hệ thống cấp Slot riêng cho xe sau khi thanh toán thành công.

- Given không còn Slot dành cho ô tô thẻ tháng  
  When Driver đăng ký thẻ tháng ô tô  
  Then hệ thống từ chối và báo hết slot thẻ tháng ô tô.

- Given vé tháng hết hạn và xe vẫn còn trong bãi  
  When hệ thống quét trạng thái lúc 00:00  
  Then hệ thống downgrade vé tháng và bắt đầu tính phí vãng lai.

- Given khách gia hạn vé tháng thành công  
  When thanh toán gia hạn được xác nhận  
  Then quyền lợi vé tháng được kích hoạt lại ngay lập tức.

- Given Driver đã có Monthly Subscription còn hiệu lực cho một Vehicle  
  When Driver muốn dùng quyền lợi đó cho Vehicle khác  
  Then hệ thống từ chối vì mỗi Monthly Subscription chỉ áp dụng cho một Vehicle.

---

## FR-007: Parking Session Tracking

### Description

Hệ thống theo dõi một lượt gửi xe từ lúc check-in đến lúc check-out.

### Actors

- Parking Staff
- Driver
- Parking Manager
- System

### Preconditions

- Parking session đã được tạo.

### Trigger

- Staff, Driver hoặc Manager cần xem trạng thái lượt gửi.

### Main Flow

1. Actor mở màn hình theo dõi lượt gửi.
2. Hệ thống hiển thị danh sách session.
3. Actor tìm theo biển số, mã gửi xe, mã booking hoặc trạng thái.
4. Hệ thống hiển thị thông tin:
    - Biển số.
    - Loại xe.
    - Giờ vào.
    - Zone/Slot.
    - Trạng thái.
    - Phí tạm tính nếu có.
5. Actor xem chi tiết session.

### Alternative Flow

- Driver chỉ xem session của chính mình.
- Manager xem toàn bộ session.
- Staff lọc session theo cổng/khu vực/trạng thái.

### Exception Flow

- Không tìm thấy session.
- Session đã check-out.
- Actor không có quyền xem session.

### Business Rules

- BR-028
- BR-029
- BR-030
- BR-031

### Postconditions

- Actor xem được thông tin session theo quyền.

### Acceptance Criteria

- Given xe đã check-in  
  When Staff tìm bằng biển số  
  Then hệ thống hiển thị session đang mở.

- Given Driver có session đang gửi  
  When Driver mở màn hình theo dõi  
  Then hệ thống hiển thị thông tin lượt gửi hiện tại.

---

## FR-008: Vehicle Check-out

### Description

Hệ thống cho phép Staff xử lý xe ra bãi, tính phí, thanh toán và cập nhật trạng thái chỗ đỗ.

### Actors

- Parking Staff
- System
- Driver

### Preconditions

- Xe có parking session đang mở.
- Staff có quyền check-out.

### Trigger

- Driver muốn rời bãi.

### Main Flow

1. Staff mở màn hình Check-out.
2. Staff nhập biển số hoặc mã gửi xe.
3. Hệ thống tìm parking session đang mở.
4. Hệ thống tính phí dựa trên:
    - Loại xe.
    - Thời gian gửi.
    - Khung giờ sáng/tối.
    - Qua đêm.
    - Nhiều ngày.
    - Chính sách thẻ tháng nếu có.
5. Driver chọn phương thức thanh toán:
    - Tiền mặt.
    - Online qua ngân hàng.
6. Hệ thống xác nhận trạng thái thanh toán:
    - Tiền mặt: Staff xác nhận.
    - Online: ngân hàng/payment gateway xác nhận.
7. Khi thanh toán thành công, hệ thống kết thúc session.
8. Hệ thống giải phóng Zone/Slot.

### Alternative Flow

- Nếu Driver có thẻ tháng hợp lệ, hệ thống áp dụng chính sách thẻ tháng.
- Nếu Driver đã thanh toán online trước, hệ thống kiểm tra trạng thái thanh toán.
- Nếu có phí phát sinh, hệ thống cộng vào tổng phí.

### Exception Flow

| Trường hợp                 | Xử lý                                       |
|----------------------------|---------------------------------------------|
| Không tìm thấy session     | Báo không có lượt gửi đang mở.              |
| Thanh toán online thất bại | Cho thanh toán lại hoặc đổi sang tiền mặt.  |
| Chưa thanh toán            | Không cho hoàn tất check-out.               |
| Sai biển số                | Staff xử lý ngoại lệ trước khi check-out.   |
| Có phí phát sinh           | Cộng vào tổng phí trước khi thanh toán.     |
| Mất mã gửi xe              | Staff xác minh bằng biển số/thông tin khác. |

### Business Rules

- BR-032
- BR-033
- BR-034
- BR-035
- BR-036
- BR-PAY-006
- BR-FEE-001
- BR-FEE-002
- BR-FEE-003
- BR-FEE-004
- BR-FEE-005
- BR-FEE-007

### Postconditions

- Hệ thống kết thúc session.
- Hệ thống giải phóng Zone/Slot.

### Acceptance Criteria

- Given xe có session đang mở  
  When Staff xác nhận check-out và thanh toán thành công  
  Then session kết thúc và chỗ đỗ được giải phóng.

---

## FR-009: Payment Management

### Description

Hệ thống hỗ trợ thanh toán tiền mặt và thanh toán online thật qua ngân hàng. Hệ thống không hỗ trợ thanh toán bằng thẻ.

### Actors

- Driver
- Parking Staff
- System
- Bank Payment Gateway

### Preconditions

- Có khoản phí cần thanh toán.
- Khoản phí thuộc một trong các nghiệp vụ: parking fee, booking deposit, monthly subscription fee hoặc phí phát sinh.

### Trigger

- Driver thanh toán phí.

### Main Flow - Cash Payment

1. Staff chọn phương thức tiền mặt.
2. Hệ thống hiển thị số tiền cần thu.
3. Staff nhận tiền từ Driver.
4. Staff xác nhận đã thu tiền.
5. Hệ thống cập nhật trạng thái Paid.
6. Nếu số tiền cần thanh toán có số lẻ, discount hoặc VAT, hệ thống áp dụng cash rounding rule.
7. Hệ thống hiển thị số tiền sau làm tròn để Staff thu tiền.

### Main Flow - Bank Online Payment

1. Driver chọn thanh toán online qua ngân hàng.
2. Hệ thống tạo yêu cầu thanh toán.
3. Driver thực hiện thanh toán.
4. Bank Payment Gateway trả kết quả.
5. Nếu thành công, hệ thống cập nhật trạng thái Paid.
6. Nếu thất bại, hệ thống giữ trạng thái Pending.

### Alternative Flow

- Cho thanh toán lại hoặc đổi sang tiền mặt nếu nghiệp vụ cho phép.

### Exception Flow

| Trường hợp                   | Xử lý                                                             |
|------------------------------|-------------------------------------------------------------------|
| Thanh toán online thất bại   | Cho thanh toán lại hoặc đổi sang tiền mặt nếu nghiệp vụ cho phép. |
| Thanh toán chưa xác nhận     | Không hoàn tất check-out/booking/monthly subscription.            |
| Người dùng chọn card payment | Không hiển thị hoặc báo không hỗ trợ.                             |
| Staff xác nhận sai           | Cần xử lý điều chỉnh theo quyền quản lý.                          |
| Booking quá thời gian thanh toán | Hệ thống tự động hủy booking và trả slot/capacity về available. |
| Cần hoàn cọc booking | Hệ thống tạo trạng thái refund/refunded theo chính sách hoàn tiền. |

### Business Rules

- BR-PAY-001
- BR-PAY-002
- BR-PAY-003
- BR-PAY-004
- BR-PAY-005
- BR-PAY-006
- BR-PAY-007

### Postconditions

- Khoản phí được cập nhật trạng thái thanh toán.
- Nghiệp vụ liên quan chỉ tiếp tục khi payment hợp lệ.

### Acceptance Criteria

- Given Driver chọn online banking  
  When ngân hàng xác nhận thành công  
  Then hệ thống cập nhật payment là Paid.

- Given Driver chọn thanh toán bằng thẻ  
  When hệ thống hiển thị phương thức thanh toán  
  Then card payment không xuất hiện.

- Given khách thanh toán tiền mặt và số tiền cần làm tròn  
  When hệ thống áp dụng cash rounding rule  
  Then số tiền thu được làm tròn theo cash_rounding_unit và rounding_threshold.

- Given khách thanh toán online  
  When hệ thống tạo giao dịch  
  Then số tiền thanh toán giữ nguyên giá trị chính xác, không làm tròn.

---

## FR-010: Fee Calculation

### Description

Hệ thống tính phí gửi xe dựa trên thời gian thực tế sử dụng, loại xe, pricing window, base duration, base price, increment block, increment price, window cap, grace period và các phụ phí/phí phạt nếu có.

### Actors

- Parking Staff
- Driver
- System

### Preconditions

- Parking session có thời gian check-in.
- Có bảng giá hợp lệ theo loại xe và pricing window.
- Các biến cấu hình tính phí đã được thiết lập.

### Trigger

- Staff thực hiện check-out.
- Driver xem phí tạm tính.
- Hệ thống cần tính tổng phí.

### Main Flow

1. Hệ thống lấy thông tin parking session.
2. Hệ thống xác định loại xe.
3. Hệ thống xác định thời gian check-in và check-out.
4. Hệ thống xác định các pricing window mà session đi qua.
5. Nếu session đi qua nhiều pricing window, hệ thống tách session thành các đoạn thời gian tương ứng.
6. Với từng đoạn thời gian, hệ thống áp dụng:
   - Base Duration.
   - Base Price.
   - Increment Block.
   - Increment Price.
   - Window Cap.
7. Nếu thời gian phát sinh nhỏ hơn hoặc bằng grace period, hệ thống không tính thêm block mới.
8. Nếu thời gian phát sinh lớn hơn grace period, hệ thống tính thành block mới.
9. Nếu session có booking deposit, hệ thống trừ deposit khỏi số tiền cần thanh toán theo chính sách booking.
10. Nếu session có phụ phí hoặc phí phạt, hệ thống cộng vào tổng phí.
11. Nếu thanh toán tiền mặt, hệ thống áp dụng cash rounding rule.
12. Nếu thanh toán online, hệ thống giữ nguyên giá trị chính xác.
13. Hệ thống hiển thị tổng phí cần thanh toán.

### Alternative Flow

- Nếu xe có vé tháng hợp lệ, hệ thống áp dụng chính sách vé tháng.
- Nếu vé tháng hết hạn trong lúc xe vẫn còn trong bãi, hệ thống tính phí vãng lai từ thời điểm hết hạn.
- Nếu xe check-out trễ hơn thời gian booking, phần phát sinh được tính theo block pricing của bảng giá vãng lai.
- Nếu có lost card penalty hoặc wrong zone penalty, hệ thống cộng vào tổng phí.

### Exception Flow

| Trường hợp | Xử lý |
|---|---|
| Thiếu bảng giá | Hệ thống báo không thể tính phí. |
| Thiếu pricing window | Hệ thống báo thiếu cấu hình khung giờ. |
| Thiếu base duration/base price/increment block/increment price | Hệ thống báo thiếu cấu hình bảng giá. |
| Thời gian check-out nhỏ hơn check-in | Hệ thống báo lỗi dữ liệu. |
| Không xác định được loại xe | Hệ thống yêu cầu cập nhật thông tin. |
| Thiếu cấu hình grace period hoặc rounding | Hệ thống báo thiếu cấu hình chính sách tính phí. |

### Business Rules

- BR-FEE-001
- BR-FEE-002
- BR-FEE-003
- BR-FEE-004
- BR-FEE-005
- BR-FEE-006
- BR-FEE-007
- BR-FEE-008
- BR-FEE-009
- BR-FEE-010
- BR-FEE-011
- BR-FEE-012
- BR-FEE-013
- BR-FEE-014
- BR-FEE-015

### Postconditions

- Tổng phí được tính và hiển thị.
- Payment record có thể được tạo dựa trên tổng phí.
- Nếu có rounding, hệ thống lưu cả số tiền gốc và số tiền sau làm tròn nếu cần đối soát.

### Acceptance Criteria

- Given xe có session hợp lệ  
  When Staff check-out  
  Then hệ thống tính phí theo loại xe và thời gian thực tế sử dụng.

- Given session đi qua nhiều pricing window  
  When hệ thống tính phí  
  Then hệ thống tách session theo từng pricing window và áp dụng bảng giá riêng cho từng window.

- Given phí trong một pricing window vượt window cap  
  When hệ thống tính phí window đó  
  Then phí của window đó không vượt quá window cap.

- Given session đi qua ngày mới  
  When hệ thống tính phí  
  Then session không bị reset và tiếp tục được tính theo pricing window tương ứng.

- Given thời gian phát sinh nhỏ hơn hoặc bằng grace period  
  When hệ thống tính block phát sinh  
  Then hệ thống không tính block mới.

- Given thời gian phát sinh lớn hơn grace period  
  When hệ thống tính block phát sinh  
  Then hệ thống tính thành block mới.

- Given khách thanh toán tiền mặt và số tiền cần làm tròn  
  When hệ thống tính tổng phí  
  Then hệ thống áp dụng cash rounding rule.

- Given khách thanh toán online  
  When hệ thống tính tổng phí  
  Then hệ thống giữ nguyên số tiền chính xác, không làm tròn.

---

## FR-011: Exception Handling

### Description

Hệ thống hỗ trợ Staff xử lý các tình huống ngoại lệ trong vận hành.

### Actors

- Parking Staff
- Parking Manager
- System

### Preconditions

- Có tình huống ngoại lệ phát sinh.
- Staff có quyền xử lý.

### Trigger

- Có tình huống ngoại lệ phát sinh.

### Supported Exceptions

| Exception                  | Mô tả                                        |
|----------------------------|----------------------------------------------|
| Lost Virtual Card Code | Driver mất mã gửi xe/mã thẻ mô phỏng. |
| Wrong Plate Number | Biển số nhập sai hoặc không khớp. |
| Overdue Parking | Xe gửi quá thời gian dự kiến/booking. |
| Wrong Zone Parking | Xe đỗ sai khu vực quy định hoặc quá thời gian cho phép ở khu vực đó. |
| Unpaid Session | Xe chưa thanh toán. |
| Slot Occupied Unexpectedly | Slot đã bị chiếm hoặc trạng thái không đúng. |
| Lost Card Penalty | Phí phạt áp dụng khi Staff chuyển trạng thái vé thành LOST. |
| Wrong Zone Penalty | Phí phạt áp dụng khi xe đỗ sai khu vực và được Staff xác nhận. |

### Main Flow

1. Staff mở session hoặc booking có vấn đề.
2. Staff chọn loại ngoại lệ.
3. Hệ thống hiển thị thông tin liên quan.
4. Staff nhập lý do xử lý.
5. Hệ thống yêu cầu xác nhận.
6. Hệ thống cập nhật trạng thái hoặc phí phát sinh nếu có.
7. Hệ thống lưu kết quả xử lý.
8. Nếu ngoại lệ phát sinh phí phạt, hệ thống cộng phí phạt vào tổng phí cần thanh toán.
9. Nếu là mất vé/mã gửi xe, hệ thống chỉ cho phép xe rời bãi sau khi khách thanh toán phí gửi xe hiện tại và lost card penalty.

### Alternative Flow

- Một số ngoại lệ cần Manager xác nhận.

### Exception Flow

- Xe chưa thanh toán không được hoàn tất check-out, trừ khi Manager xử lý đặc biệt.
- Sai biển số phải được chỉnh trước khi hoàn tất session.

### Business Rules

- BR-042
- BR-043
- BR-044
- BR-045
- BR-046

### Postconditions

- Hệ thống cập nhật trạng thái hoặc phí phát sinh nếu có.
- Hệ thống lưu kết quả xử lý.

### Acceptance Criteria

- Given session có lỗi sai biển số  
  When Staff cập nhật và xác nhận lý do  
  Then hệ thống lưu thông tin đã chỉnh sửa.

- Given xe chưa thanh toán  
  When Staff cố check-out  
  Then hệ thống chặn hoàn tất check-out.

- Given Staff chuyển trạng thái vé thành LOST  
  When hệ thống tính phí check-out  
  Then hệ thống cộng phí gửi xe hiện tại và lost card penalty vào tổng phí.

- Given xe bị xác nhận đỗ sai khu vực  
  When Staff xử lý ngoại lệ  
  Then hệ thống cộng wrong zone penalty nếu chính sách áp dụng.

---

## FR-012: Operation Monitoring

### Description

Hệ thống cho phép Manager theo dõi tình trạng vận hành của bãi xe.

### Actors

- Parking Manager
- System

### Preconditions

- Manager đã đăng nhập.
- Có dữ liệu vận hành.

### Trigger

- Manager mở dashboard vận hành.

### Main Flow

1. Manager mở dashboard vận hành.
2. Hệ thống hiển thị:
    - Tổng số Zone/Slot.
    - Số chỗ còn trống.
    - Số chỗ đang sử dụng.
    - Số chỗ đã reserved.
    - Số chỗ bảo trì/tạm khóa.
    - Lượt xe vào/ra.
    - Doanh thu.
    - Tỷ lệ lấp đầy.
3. Manager lọc theo Building, Floor, Zone, loại xe hoặc thời gian.
4. Hệ thống cập nhật kết quả hiển thị.

### Alternative Flow

- Manager lọc theo Building, Floor, Zone, loại xe hoặc thời gian.

### Exception Flow

- Không có dữ liệu vận hành.

### Business Rules

- BR-047
- BR-048
- BR-049
- BR-050
- BR-051

### Postconditions

- Hệ thống hiển thị tình trạng bãi xe và doanh thu tương ứng.

### Acceptance Criteria

- Given Manager mở dashboard  
  When hệ thống có dữ liệu session/payment  
  Then hệ thống hiển thị tình trạng bãi xe và doanh thu tương ứng.

---
# 6. Business Rules

## 6.1 Parking Structure Rules

| Rule ID | Rule                                                                |
|---------|---------------------------------------------------------------------|
| BR-001  | Building có thể có nhiều Floor.                                     |
| BR-002  | Floor có thể có nhiều Zone.                                         |
| BR-003  | Zone có thể dùng cho xe máy hoặc ô tô tùy cấu hình.                 |
| BR-004  | Xe máy được quản lý theo sức chứa của Zone.                         |
| BR-005  | Ô tô được quản lý theo từng Slot.                                   |
| BR-006  | Zone/Slot bị khóa hoặc bảo trì không được dùng để check-in/booking. |
| BR-007  | Zone ô tô phải phân loại `zone_access_type` là `GENERAL` hoặc `MONTHLY`. |
| BR-008  | Slot status chỉ phản ánh trạng thái vật lý/vận hành, không phản ánh quyền giữ Slot tháng. |

---

## 6.2 Hardware Simulation Rules

| Rule ID   | Rule                                  | Cách xử lý                                                                          |
|-----------|---------------------------------------|-------------------------------------------------------------------------------------|
| BR-HW-001 | Hệ thống không có camera thật.        | Biển số được nhập thủ công trên web.                                                |
| BR-HW-002 | Hệ thống không có thẻ vật lý thật.    | Dùng mã gửi xe/mã thẻ mô phỏng.                                                     |
| BR-HW-003 | Hệ thống không có cảm biến slot thật. | Trạng thái Zone/Slot được cập nhật qua thao tác check-in/check-out/booking/manager. |
| BR-HW-004 | Hệ thống không có barrier thật.       | Trạng thái cho phép ra/vào chỉ là trạng thái nghiệp vụ trong hệ thống.              |

---

## 6.3 Driver Account & Vehicle Rules

| Rule ID    | Rule                                                                        | Cách xử lý                                                         |
|------------|-----------------------------------------------------------------------------|--------------------------------------------------------------------|
| BR-ACC-001 | Tài khoản Driver có thể được tạo mà chưa cần đăng ký xe.                    | Cho phép account tồn tại với danh sách xe rỗng.                    |
| BR-ACC-002 | Một Driver Account có thể có nhiều xe.                                      | Driver được thêm nhiều biển số/xe vào tài khoản.                   |
| BR-ACC-003 | Xe có thể được thêm sau khi tài khoản đã được tạo.                          | Cung cấp chức năng Add Vehicle.                                    |
| BR-ACC-004 | Khi booking hoặc đăng ký thẻ tháng, hệ thống bắt buộc có thông tin biển số. | Nếu tài khoản chưa có xe, yêu cầu thêm xe hoặc nhập biển số trước. |

---

## 6.4 Parking Allocation Rules

| Rule ID      | Rule                                                                                                   | Cách xử lý                                                                     |
|--------------|--------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| BR-ALLOC-001 | Khi check-in, xe máy được gợi ý theo Zone/Area còn chỗ.                                                | Hệ thống tìm Zone còn capacity phù hợp.                                        |
| BR-ALLOC-002 | Khi check-in, ô tô Walk-in/Booking chỉ được gán Slot trong Zone `GENERAL`. | Hệ thống tìm Slot còn trống, usable, không bảo trì trong Zone `GENERAL`. |
| BR-ALLOC-003 | Ô tô booking không dùng Slot do Driver chọn trước. | Hệ thống gán Slot thực tế trong Zone `GENERAL` khi check-in và lưu ở Parking Session. |
| BR-ALLOC-004 | Ô tô thẻ tháng được gán theo Slot riêng đã cấp trong Zone `MONTHLY`. | Hệ thống dùng `monthly_subscription.assigned_slot_id`. |
| BR-ALLOC-005 | Zone/Slot đang bảo trì hoặc tạm khóa không được phân bổ.                                               | Hệ thống loại khỏi danh sách gợi ý.                                            |
| BR-ALLOC-006 | Walk-in/Booking không được dùng capacity hoặc Slot đã bảo vệ cho Monthly Subscription. | Xe máy trừ active motorcycle subscriptions khỏi general capacity; ô tô general chỉ dùng Zone `GENERAL`. |
| BR-ALLOC-007 | Hệ thống phải tránh trạng thái lấp đầy 100% nếu điều đó làm mất quyền đảm bảo chỗ của khách thẻ tháng. | Khi capacity chạm ngưỡng giới hạn, hệ thống từ chối nhận thêm walk-in/booking. |

---

## 6.5 Booking Rules

| Rule ID     | Rule                                                                                                  | Cách xử lý                                                                                 |
|-------------|-------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| BR-BOOK-001 | Booking bắt buộc nhập biển số xe.                                                                     | Nếu thiếu biển số, không cho tạo booking.                                                  |
| BR-BOOK-002 | Booking chỉ hợp lệ sau khi thanh toán cọc thành công.                                                 | Trạng thái ban đầu là Pending; sau khi cọc thành công chuyển thành Confirmed.       |
| BR-BOOK-003 | Booking phải được đặt trước tối thiểu 1 tiếng tính từ thời điểm thanh toán cọc thành công.            | Nếu thời gian dự kiến vào bãi nhỏ hơn 1 tiếng sau khi thanh toán, hệ thống từ chối booking. |
| BR-BOOK-004 | Booking chỉ được đặt trước tối đa 8 tiếng tính từ thời điểm thanh toán cọc thành công.                | Nếu thời gian dự kiến vào bãi lớn hơn 8 tiếng sau khi thanh toán, hệ thống từ chối booking. |
| BR-BOOK-006 | Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành. | Hệ thống xác định bảng giá theo loại xe và thời điểm booking để tính deposit. |
| BR-BOOK-007 | Booking phải chọn Building trước. | Hệ thống kiểm tra general capacity trong Building đã chọn. |
| BR-BOOK-008 | Xe máy booking không được cho Driver chọn Zone hoặc Slot. | Hệ thống giữ một general motorcycle capacity unit ở Building và gán Zone khi check-in. |
| BR-BOOK-009 | Ô tô booking không được cho Driver chọn Zone hoặc Slot. | Hệ thống giữ general car capacity ở Building và gán Slot trong Zone `GENERAL` khi check-in. |
| BR-BOOK-010 | Booking không được dùng phần capacity/slot đã giữ cho nhóm thẻ tháng. | Hệ thống trừ active motorcycle subscriptions khỏi motorcycle general capacity và loại Zone `MONTHLY` khỏi car booking. |
| BR-BOOK-010 | Nếu khách hủy booking trước giờ booking ít nhất 1 tiếng, khách được hoàn cọc.                         | Hệ thống chuyển booking sang Cancelled và tạo trạng thái Refund/Refunded.                  |
| BR-BOOK-011 | Nếu khách hủy booking trễ hơn thời hạn được hoàn cọc, khách mất cọc.                                  | Hệ thống chuyển booking sang Cancelled và giữ cọc.                                         |
| BR-BOOK-012 | Nếu khách đến trễ quá 45 phút so với giờ booking, booking bị hủy và mất cọc.                          | Hệ thống chuyển booking sang No-show/Cancelled và giữ cọc.                                 |
| BR-BOOK-013 | Nếu khách đến đúng hạn hoặc trong thời gian trễ cho phép, booking được dùng để tạo parking session.   | Hệ thống chuyển booking thành session khi check-in thành công.                             |
| BR-BOOK-014 | Nếu khách đến sớm hơn giờ booking, hệ thống cho phép check-in sớm nếu còn chỗ phù hợp.                | Thời gian gửi xe bắt đầu tính từ thời điểm check-in thực tế.                               |
| BR-BOOK-015 | Nếu khách gửi xe trong phạm vi thời gian đã đặt, khách thanh toán phần phí còn lại sau khi trừ cọc.   | Khi check-out, hệ thống thu phần còn lại, thường là 70% phí dự kiến.                       |
| BR-BOOK-016 | Nếu khách gửi vượt quá thời gian đã đặt, phần vượt được tính phí theo chính sách gửi xe thông thường. | Hệ thống cộng phí vượt giờ vào tổng phí cần thanh toán.                                    |
| BR-BOOK-017 | Một biển số không nên có nhiều booking active trong cùng khoảng thời gian.                            | Hệ thống kiểm tra trùng lịch booking theo biển số.                                         |
| BR-BOOK-018 | Booking chưa thanh toán trong booking_payment_timeout_minutes sẽ tự động bị hủy. | Hệ thống chuyển booking sang CANCELLED và trả general capacity đã giữ; không đổi `slot_status`. |
| BR-BOOK-019 | Sau khi thanh toán booking thành công, general capacity được giữ cho booking ở mức Building. | Không lưu Driver-selected Zone/Slot và không chuyển Slot sang `RESERVED`. |
| BR-BOOK-020 | Nếu khách không check-in trong checkin_grace_minutes, booking bị hủy tự động. | Hệ thống chuyển booking sang EXPIRED hoặc CANCELLED và không hoàn Deposit Fee. |
| BR-BOOK-021 | Nếu khách check-out trễ hơn thời gian booking, phần phát sinh được tính theo block pricing của bảng giá vãng lai. | Hệ thống cộng phí overtime vào tổng phí cần thanh toán. |

---

## 6.6 Monthly Subscription Rules

| Rule ID      | Rule                                                                              | Cách xử lý                                                                                 |
|--------------|-----------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| BR-MONTH-001 | Mỗi Monthly Subscription xe máy `ACTIVE` giữ một capacity unit tại Building đã đăng ký. | Hệ thống trừ số active motorcycle subscriptions khỏi capacity Walk-in/Booking xe máy. |
| BR-MONTH-002 | Thẻ tháng xe máy không phân theo Zone/Slot cụ thể. | Khi xe vào bãi, hệ thống gợi ý Zone còn chỗ và dùng capacity tháng đã giữ. |
| BR-MONTH-003 | Thẻ tháng ô tô được cấp Slot riêng trong Zone `MONTHLY`. | Hệ thống gắn xe ô tô thẻ tháng với `monthly_subscription.assigned_slot_id`. |
| BR-MONTH-004 | Thẻ tháng không giới hạn số lượt vào/ra mỗi ngày.                                 | Cho phép nhiều parking session trong ngày nếu thẻ còn hiệu lực và không có session đang mở. |
| BR-MONTH-005 | Một xe không được có nhiều thẻ tháng còn hiệu lực trùng thời gian.                | Hệ thống kiểm tra biển số trước khi đăng ký/gia hạn.                                       |
| BR-MONTH-006 | Thẻ tháng chỉ được kích hoạt sau khi thanh toán thành công.                       | Nếu chưa thanh toán, trạng thái là Pending.                                         |
| BR-MONTH-007 | Không được cấp thêm thẻ tháng xe máy nếu active motorcycle subscriptions đã dùng hết capacity usable. | Hệ thống từ chối đăng ký và báo hết capacity tháng xe máy. |
| BR-MONTH-008 | Không được cấp thêm thẻ tháng ô tô nếu không còn Slot usable trong Zone `MONTHLY`. | Hệ thống từ chối đăng ký và báo hết slot thẻ tháng ô tô. |
| BR-MONTH-009 | Capacity/Slot dành cho thẻ tháng phải được bảo vệ khỏi Walk-in và Booking. | Xe máy dùng công thức dynamic capacity; ô tô dùng Zone `GENERAL`/`MONTHLY` cố định. |
| BR-MONTH-010 | Vé tháng áp dụng cho biển số đăng ký cố định. | Hệ thống kiểm tra biển số khi check-in để xác định quyền lợi vé tháng. |
| BR-MONTH-011 | Vé tháng được thanh toán trả trước theo chu kỳ. | Vé chỉ active sau khi thanh toán thành công. |
| BR-MONTH-012 | Vé tháng có số ngày hiệu lực cấu hình. | Hệ thống xác định ngày bắt đầu và ngày hết hạn. |
| BR-MONTH-013 | Mỗi vé tháng chỉ áp dụng cho một xe đã đăng ký. | Không cho dùng cùng một vé tháng cho nhiều xe. |
| BR-MONTH-014 | Hệ thống quét vé tháng hết hạn lúc 00:00 mỗi ngày. | Nếu vé hết hạn và xe vẫn còn trong bãi, hệ thống downgrade. |
| BR-MONTH-015 | Khi vé tháng bị downgrade, xe bắt đầu bị tính phí vãng lai từ thời điểm hết hạn. | Áp dụng bảng giá vãng lai cho đến khi xe rời bãi. |
| BR-MONTH-016 | Sau khi gia hạn thành công, quyền lợi vé tháng được kích hoạt lại ngay lập tức. | Hệ thống cập nhật lại trạng thái active. |
| BR-MONTH-017 | Monthly Subscription phải gán với một Card `MONTHLY` khi quyền lợi được cấp. | `monthly_subscription.assigned_card_id` trỏ tới Card `MONTHLY`; Card này không trở về `AVAILABLE` sau mỗi check-out. |
| BR-MONTH-018 | Monthly Subscription `PENDING` có thể chưa có thời điểm kích hoạt/hết hạn. | `activated_at` và `expired_at` được phép `NULL` cho đến khi thanh toán thành công. |
| BR-MONTH-019 | Card type không tự cấp quyền lợi tháng. | Quyền lợi tháng chỉ áp dụng khi có Monthly Subscription hợp lệ cùng Vehicle và Building. |
| BR-MONTH-020 | Slot tháng ô tô không được biểu diễn bằng `slot_status`. | Quyền giữ Slot nằm ở `monthly_subscription.assigned_slot_id`; `slot_status` chỉ phản ánh AVAILABLE/OCCUPIED/BLOCKED/MAINTENANCE. |

---

## 6.7 Payment Rules

| Rule ID    | Rule                                                                                                               | Cách xử lý                                                                |
|------------|--------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------|
| BR-PAY-001 | Hệ thống hỗ trợ thanh toán tiền mặt.                                                                               | Staff xác nhận đã thu tiền trên hệ thống.                                 |
| BR-PAY-002 | Hệ thống hỗ trợ thanh toán online thật qua ngân hàng.                                                              | Hệ thống nhận kết quả thanh toán từ kênh ngân hàng/payment gateway.       |
| BR-PAY-003 | Hệ thống không hỗ trợ thanh toán bằng thẻ.                                                                         | Không hiển thị phương thức card payment.                                  |
| BR-PAY-004 | Booking phải thanh toán Deposit Fee trước. | Booking chỉ confirmed khi Deposit Fee được thanh toán thành công.         |
| BR-PAY-005 | Nếu booking bị hủy do khách đến trễ quá 45 phút, khách mất cọc.                                                    | Không hoàn cọc cho trạng thái No-show.                                    |
| BR-PAY-006 | Check-out chỉ hoàn tất khi khoản phí cần thanh toán đã được xác nhận.                                              | Nếu pending, hệ thống chặn check-out.                                     |
| BR-PAY-007 | Thẻ tháng chỉ active sau khi thanh toán thành công.                                                                | Nếu payment failed, thẻ tháng không có hiệu lực.                          |
| BR-PAY-008 | Nếu khách hủy booking trước giờ booking ít nhất 1 tiếng, khách được hoàn cọc.                                      | Hệ thống tạo trạng thái Refund/Refunded cho khoản cọc.                    |
| BR-PAY-009 | Nếu khách đã đặt cọc booking, khi check-out hệ thống trừ cọc khỏi tổng phí cần thanh toán theo chính sách booking. | Khách chỉ thanh toán phần còn lại hoặc phần phát sinh nếu có.             |
| BR-PAY-008 | Booking chưa thanh toán trong booking_payment_timeout_minutes sẽ bị hủy tự động. | Hệ thống chuyển booking sang CANCELLED và trả general capacity đã giữ; không đổi Slot status. |
| BR-PAY-009 | Thanh toán tiền mặt áp dụng cash rounding rule nếu cần. | Làm tròn theo cash_rounding_unit và rounding_threshold.                   |
| BR-PAY-010 | Thanh toán online không làm tròn. | Giữ nguyên giá trị chính xác của giao dịch.                               |
| BR-PAY-011 | Nếu cần hoàn Deposit Fee, hệ thống ghi nhận trạng thái refund/refunded. | Áp dụng cho các trường hợp được hoàn theo chính sách booking.             |

---

## 6.8 Fee Calculation Rules

| Rule ID | Rule | Cách xử lý |
|---|---|---|
| BR-FEE-001 | Phí gửi xe phụ thuộc vào loại xe. | Hệ thống áp dụng bảng giá theo vehicle_type. |
| BR-FEE-002 | Phí gửi xe được tính theo thời gian thực tế sử dụng. | Hệ thống tính từ check-in time đến check-out time. |
| BR-FEE-003 | Hệ thống áp dụng mô hình pricing window. | Mỗi khung giờ có bảng giá riêng. |
| BR-FEE-004 | Mỗi pricing window có base duration và base price. | Nếu thời gian nằm trong base duration, áp dụng base price. |
| BR-FEE-005 | Sau khi vượt base duration, hệ thống tính thêm theo increment block. | Mỗi block phát sinh áp dụng increment price. |
| BR-FEE-006 | Nếu session đi qua nhiều pricing window, hệ thống tách session theo từng window. | Mỗi đoạn thời gian được tính theo bảng giá riêng. |
| BR-FEE-007 | Window Cap chỉ áp dụng cho từng pricing window riêng biệt. | Không dùng window cap để giới hạn toàn bộ session. |
| BR-FEE-008 | Hệ thống vận hành 24/7 và không reset session khi qua ngày mới. | Session tiếp tục được tính theo pricing window của ngày tiếp theo. |
| BR-FEE-009 | Deposit Fee của booking bằng giá của block đầu tiên theo bảng giá hiện hành. | Hệ thống tính deposit dựa trên bảng giá tại thời điểm booking. |
| BR-FEE-010 | Nếu khách gửi xe vượt thời gian booking, phần vượt tính theo block pricing của bảng giá vãng lai. | Hệ thống cộng phí overtime vào tổng phí. |
| BR-FEE-011 | Nếu thời gian phát sinh nhỏ hơn hoặc bằng grace_period_minutes, hệ thống không tính block mới. | Không cộng increment price cho phần thời gian này. |
| BR-FEE-012 | Nếu thời gian phát sinh lớn hơn grace_period_minutes, hệ thống tính thành block mới. | Cộng thêm increment price tương ứng. |
| BR-FEE-013 | Thanh toán tiền mặt áp dụng cash rounding nếu có số lẻ, discount hoặc VAT. | Hệ thống làm tròn theo cash_rounding_unit và rounding_threshold. |
| BR-FEE-014 | Thanh toán online không làm tròn. | Hệ thống giữ nguyên giá trị chính xác. |
| BR-FEE-015 | Nếu có phí phạt hoặc phụ phí, hệ thống cộng vào tổng phí trước khi thanh toán. | Áp dụng lost card penalty, wrong zone penalty hoặc phí phát sinh khác nếu có. |

---

## 6.9 Parking Session Rules

| Rule ID | Rule                                                   |
|---------|--------------------------------------------------------|
| BR-028  | Session bắt đầu khi check-in thành công.               |
| BR-029  | Session kết thúc khi check-out và thanh toán hoàn tất. |
| BR-030  | Session đang mở phải giữ Zone/Slot tương ứng.          |
| BR-031  | Driver chỉ được xem thông tin của mình.                |

---

## 6.10 Vehicle Check-out Rules

| Rule ID | Rule                                                                         |
|---------|------------------------------------------------------------------------------|
| BR-032  | Xe chỉ được check-out nếu có session đang mở.                                |
| BR-033  | Check-out phải xác nhận thanh toán trước khi hoàn tất.                       |
| BR-034  | Sau check-out, Zone/Slot được giải phóng.                                    |
| BR-035  | Nếu có thẻ tháng hợp lệ, phí được xử lý theo chính sách thẻ tháng.           |
| BR-036  | Nếu phát sinh lỗi thông tin, Staff phải xử lý exception trước khi cho xe ra. |

---

## 6.11 Exception Handling Rules

| Rule ID | Rule                                                                              |
|---------|-----------------------------------------------------------------------------------|
| BR-042  | Ngoại lệ phải có lý do xử lý.                                                     |
| BR-043  | Một số ngoại lệ cần Manager xác nhận.                                             |
| BR-044  | Xe chưa thanh toán không được hoàn tất check-out, trừ khi Manager xử lý đặc biệt. |
| BR-045  | Sai biển số phải được chỉnh trước khi hoàn tất session.                           |
| BR-046  | Gửi sai khu vực có thể phát sinh cảnh báo hoặc phí tùy chính sách.                |
| BR-052 | Khi Staff chuyển trạng thái vé/mã gửi xe thành LOST, hệ thống áp dụng lost card penalty. |
| BR-053 | Xe mất vé/mã gửi xe chỉ được rời bãi sau khi thanh toán phí gửi xe hiện tại và lost card penalty. |
| BR-054 | Xe đỗ sai khu vực có thể bị áp dụng wrong zone penalty nếu được Staff xác nhận. |
| BR-055 | Phí phạt được cộng vào tổng phí trước khi thanh toán. |

---

## 6.12 Operation Monitoring Rules

| Rule ID | Rule                                            |
|---------|-------------------------------------------------|
| BR-047  | Manager được xem toàn bộ dữ liệu vận hành.      |
| BR-048  | Dashboard phải phân biệt xe máy và ô tô.        |
| BR-049  | Xe máy hiển thị theo Zone capacity.             |
| BR-050  | Ô tô hiển thị theo Slot status.                 |
| BR-051  | Doanh thu dựa trên các giao dịch đã thanh toán. |

---

## 6.13 System State Rules

### Permission Status

| Status | Ý nghĩa |
|---|---|
| ACTIVE | Quyền đang được sử dụng |
| INACTIVE | Quyền tạm ngừng sử dụng |

### Account Status

| Status | Ý nghĩa |
|---|---|
| ACTIVE | Tài khoản đang hoạt động bình thường |
| SUSPENDED | Tài khoản bị khóa, đình chỉ tạm thời|
| ARCHIVED | Tài khoản không còn được sử dụng |


### Building Status

| Status | Ý nghĩa |
|---|---|
| INACTIVE     | Nhà xe chưa được đưa vào sử dụng.                                              |
| ACTIVE       | Nhà xe hoạt động, cho phép xe ra vào.                                          |
| MAINTENANCE  | Bãi xe được bảo trì; không tiếp nhận xe mới. Cho phép xe đang gửi ra khỏi bãi |
| OUTOFSERVICE | Nhà xe không thể hoạt động                                                     |

### Floor Status

| Status | Ý nghĩa |
|---|---|
| INACTIVE     | Tầng chưa được đưa vào sử dụng                                              |
| ACTIVE       | Tầng được sử dụng và cho phép xe sử dụng các vị trí đỗ                      |
| MAINTENANCE  | Tầng được bảo trì; không tiếp nhận xe mới. Cho phép xe đang gửi ra khỏi bãi |
| OUTOFSERVICE | Hết hạn khi xe vẫn còn trong bãi và bị chuyển sang tính phí vãng lai        |

### Zone Status

| Status | Ý nghĩa |
|---|---|
| AVAILABLE | Khu vực trống              |
| OCCUPIED  | Khu vực không nhận thêm xe |
| BLOCKED   | Khu vực không hoạt động    |

### Slot Status

| Status | Ý nghĩa |
|---|---|
| AVAILABLE | Slot trống           |
| OCCUPIED  | Slot có xe           |
| BLOCKED   | Slot bị khóa         |
| MAINTENANCE | Slot đang bảo trì |

Slot status chỉ phản ánh trạng thái vật lý/vận hành. Slot được cấp cho ô tô thẻ tháng vẫn có thể là `AVAILABLE` khi xe đang ở ngoài bãi; quyền giữ Slot được xác định bằng `monthly_subscription.assigned_slot_id`.

### Vehicle Type Status

| Status | Ý nghĩa |
|---|---|
| INACTIVE | Loại xe đang được hỗ trợ |
| ACTIVE | Loại xe tạm ngừng hỗ trợ |

### Vehicle Status

| Status | Ý nghĩa |
|---|---|
| INACTIVE | Chưa đăng ký xe trên hệ thống |
| ACTIVE | Xe sử dụng trên hệ thống |
| PENDING | Xe mới đăng ký, đang chờ xác minh |
| SUSPENDED | Xe bị tạm khóa |
| ARCHIVED | Người dùng đã xóa hoặc không còn sử dụng xe|

### Card Status

| Status | Ý nghĩa |
|---|---|
| AVAILABLE | Card đang rảnh và có thể cấp cho một session mới |
| ASSIGNED | Card đang được cấp cho một Parking Session `NORMAL` đang mở hoặc được gán dài hạn cho một Monthly Subscription |
| LOST | Card bị báo mất |
| BLOCKED | Card bị khóa và không được sử dụng |

Card type hợp lệ gồm `NORMAL` và `MONTHLY`. Card `NORMAL` được cấp tạm thời cho Walk-in/Booking và trả về `AVAILABLE` sau check-out. Card `MONTHLY` được gán với Monthly Subscription, được Driver giữ qua nhiều lượt vào/ra và không tự cấp quyền lợi nếu subscription không hợp lệ.

### Parking Session Status

| Status | Ý nghĩa |
|---|---|
| ACTIVE | Session đang mở, xe đã check-in và vẫn còn trong bãi. Zone/Slot tương ứng đang bị giữ |
| COMPLETED  | Session đã kết thúc, xe đã check-out và thanh toán hoàn tất. Zone/Slot được giải phóng |
| LOST  | Người gửi xe bị mất vé/mã gửi xe. Hệ thống xử lý theo luồng lost card penalty trước khi cho xe ra |
| EXPIRED   | Vé/session hết hạn hoặc không còn hợp lệ theo chính sách thời gian. Trong tài liệu chưa mô tả sâu cho parking session,nhưng có liên hệ với trường hợp vé hết hạn |
| DOWNGRADED | Quyền lợi thẻ tháng bị downgrade, thường xảy ra khi thẻ tháng hết hạn trong lúc xe vẫn còn trong bãi; từ thời điểm hết hạn, hệ thống chuyển sang tính phí vãng lai |

### Booking Status

| Status | Ý nghĩa |
|---|---|
| PENDING | Chờ thanh toán |
| CONFIRMED | Đã xác nhận |
| CANCELLED | Đã hủy |
| EXPIRED | Hết hạn check-in |
| COMPLETED | Đã sử dụng |


### Incident Status

| Status | Ý nghĩa |
|---|---|
| OPEN | Sự cố mới được ghi nhận và đang chờ xử lý  |
| PROCESSING | Sự cố đang được nhân viên xử lý |
| RESOLVED | Sự cố đã được xử lý hoàn tất |
| CANCELLED | Sự cố đã bị hủy hoặc không tiếp tục xử lý |

### Monthly Subscription Status

| Status | Ý nghĩa |
|---|---|
| PENDING | Chờ thanh toán/kích hoạt |
| ACTIVE | Đang hiệu lực |
| EXPIRED | Đã hết hạn |
| DOWNGRADED | Hết hạn khi xe vẫn còn trong bãi và bị chuyển sang tính phí vãng lai |
| CANCELLED | Đã hủy |

### Pricing Policy Status

| Status | Ý nghĩa |
|---|---|
| INACTIVE | Chính sách giá chưa được áp dụng hoặc tạm ngừng |
| ACTIVE | Chính sách giá đang có hiệu lực |
| EXPIRED | Chính sách giá đã hết thời gian hiệu lực |

### Payment Status

| Status | Ý nghĩa |
|---|---|
| PENDING | Giao dịch đã được tạo và đang chờ thanh toán |
| PAID | Giao dịch đã được thanh toán thành công |
| FAILED | Giao dịch thanh toán không thành công |

## 6.14 Configurable Variables Rules

Các biến cấu hình dưới đây phải được quản lý động ở tầng cấu hình nghiệp vụ/application/admin configuration, không hard-code trực tiếp trong source code. Phiên bản physical model hiện tại không tạo thêm table riêng cho nhóm biến này.

| Key | Ý nghĩa |
|---|---|
| day_start_time | Giờ bắt đầu khung ngày. |
| night_start_time | Giờ bắt đầu khung đêm. |
| base_duration_minutes | Thời lượng cơ bản. |
| increment_block_minutes | Kích thước block. |
| grace_period_minutes | Thời gian ân hạn làm tròn block. |
| booking_payment_timeout_minutes | Timeout thanh toán booking. |
| checkin_grace_minutes | Thời gian ân hạn check-in booking. |
| lost_card_penalty | Phí mất vé/mã gửi xe. |
| wrong_zone_penalty | Phí đỗ sai khu. |
| cash_rounding_unit | Đơn vị làm tròn tiền mặt. |
| rounding_threshold | Ngưỡng làm tròn. |

## 6.13 Business Rules Summary

| Rule Group           | Nội dung chính                                                                                                                           |
|----------------------|------------------------------------------------------------------------------------------------------------------------------------------|
| Hardware Simulation  | Không có camera, thẻ thật, cảm biến, barrier; toàn bộ nhập liệu trên web.                                                                |
| Vehicle Type         | Hiện chỉ hỗ trợ xe máy và ô tô.                                                                                                          |
| Motorcycle Parking   | Xe máy được phân bổ theo Zone/Area, không theo Slot cụ thể.                                                                              |
| Car Parking          | Ô tô Walk-in/Booking chỉ dùng Slot trong Zone `GENERAL`; ô tô thẻ tháng chỉ dùng Slot đã gán trong Zone `MONTHLY`. |
| Check-in             | Chỉ tạo session khi còn chỗ hợp lệ.                                                                                                      |
| Booking | Booking yêu cầu chọn Building trước, nhập biển số/Vehicle, Deposit Fee bằng giá block đầu tiên, đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng; Driver không chọn Zone/Slot; actual Zone/Slot được gán khi check-in. |
| Monthly Subscription | Thẻ tháng xe máy đảm bảo có chỗ bằng capacity động; thẻ tháng ô tô được cấp Slot riêng trong Zone `MONTHLY`; Monthly Subscription gán với Card `MONTHLY` và Card type không tự cấp quyền lợi nếu subscription không hợp lệ. |
| Booking Cancellation | Hủy trước giờ booking ít nhất 1 tiếng được hoàn cọc; đến trễ quá 45 phút bị hủy booking và mất cọc.                                      |
| Payment              | Hỗ trợ tiền mặt và online qua ngân hàng; không hỗ trợ thanh toán bằng thẻ.                                                               |
| Fee Calculation | Phí gửi xe tính theo Time Window, Base Price, Increment/Block Pricing, Window Cap, Grace Period và trạng thái session 24/7 không reset qua ngày. |
| Exception            | Các lỗi mất mã, sai biển số, quá hạn, gửi sai khu vực, chưa thanh toán cần quy trình xử lý.                                              |
| Scale                | Có thể mở rộng Building, Floor, Zone, Slot ở mức cấu hình nghiệp vụ.                                                                     |
| Monthly Subscription Downgrade | Vé tháng hết hạn lúc xe còn trong bãi sẽ bị downgrade và bắt đầu tính phí vãng lai từ thời điểm hết hạn. |
| Rounding | Tiền mặt có thể làm tròn theo cấu hình; online payment không làm tròn. |
| Penalty | Mất vé/mã gửi xe và đỗ sai khu có thể phát sinh phí phạt. |
| Configurable Rules | Các biến pricing, timeout, grace period, penalty và rounding phải được cấu hình động ở tầng nghiệp vụ/application/admin configuration, không hard-code và không tạo thêm table riêng trong physical model hiện tại. |

---
# 7. Finalized Policy Decisions

| Policy Area                                    | Final Decision                                                                                                                                 |
|------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| Check-in Allocation                            | Khi check-in, xe máy được gợi ý Zone còn capacity; ô tô Walk-in/Booking dùng Slot trong Zone `GENERAL`; ô tô thẻ tháng dùng Slot đã cấp trong Zone `MONTHLY`. |
| Monthly Subscription - Motorcycle              | Xe máy thẻ tháng được đảm bảo có chỗ bằng capacity động: mỗi subscription `ACTIVE` làm giảm capacity Walk-in/Booking một đơn vị. |
| Monthly Subscription - Car                     | Ô tô thẻ tháng được cấp Slot riêng trong Zone `MONTHLY`; quyền giữ Slot nằm ở `monthly_subscription.assigned_slot_id`. |
| Monthly Subscription - Vehicle Binding         | Mỗi vé tháng chỉ áp dụng cho một xe đã đăng ký; không dùng `max_registered_plate`.                                                             |
| Card vs Monthly Subscription                   | Card có `card_type = NORMAL, MONTHLY`; Card `MONTHLY` gắn với Monthly Subscription, nhưng quyền lợi tháng vẫn do Monthly Subscription hợp lệ quyết định. |
| Booking - Motorcycle                           | Xe máy booking phải chọn Building và Vehicle/biển số; Driver không chọn Zone/Slot, hệ thống gán Zone khi check-in. |
| Booking - Car                                  | Ô tô booking phải chọn Building và Vehicle/biển số; Driver không chọn Zone/Slot, hệ thống gán Slot trong Zone `GENERAL` khi check-in. |
| Booking Time Limit                             | Booking phải được đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng tính từ thời điểm thanh toán cọc thành công.                                   |
| Booking Deposit                                | Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.                                                                               |
| Booking Cancellation                           | Khách hủy trước giờ booking ít nhất 1 tiếng thì được hoàn cọc.                                                                                 |
| Booking No-show                                | Khách đến trễ quá 45 phút so với giờ booking thì booking bị hủy và mất cọc.                                                                    |
| Early Arrival                                  | Nếu khách đến sớm hơn giờ booking, hệ thống cho check-in sớm nếu còn chỗ phù hợp; thời gian gửi xe bắt đầu tính từ lúc khách thực tế check-in. |
| Booking Final Payment                          | Nếu khách gửi xe trong thời gian đã đặt, khi thanh toán khách trả phần còn lại sau khi trừ cọc.                                                |
| Overtime After Booking                         | Nếu khách gửi vượt quá thời gian đã đặt, phần vượt được tính phí theo chính sách gửi xe thông thường.                                          |
| Phí qua đêm và nhiều ngày tính theo block nào? | Theo giờ / Theo ngày / Theo mốc qua đêm / Kết hợp.                                                                                             |
| Pricing Model | Hệ thống tính phí theo Time Window, Base Price, Increment/Block Pricing và Window Cap. |
| Window Cap | Window Cap chỉ áp dụng cho từng khung giờ riêng biệt, không áp dụng cho toàn bộ session. |
| 24/7 Session | Hệ thống không reset session khi qua ngày mới. |
| Monthly Subscription Downgrade | Nếu vé tháng hết hạn và xe vẫn còn trong bãi, hệ thống downgrade và tính phí vãng lai từ thời điểm hết hạn. |
| System State Management | Trạng thái hệ thống được quản lý bằng các status riêng cho Building, Floor, Zone, Slot, Vehicle, Card, Parking Session, Booking, Incident, Monthly Subscription và Pricing Policy. |
| Booking Payment Timeout | Nếu booking chưa thanh toán sau booking_payment_timeout_minutes, hệ thống tự động hủy booking. |
| Booking Check-in Grace | Nếu khách không check-in trong 45 phút sau giờ booking hoặc sau checkin_grace_minutes được cấu hình tương ứng, booking bị hủy và không hoàn Deposit Fee. |
| Overtime After Booking | Nếu khách check-out trễ hơn thời gian booking, phần phát sinh tính theo block pricing của bảng giá vãng lai. |
| Time Rounding | Nếu thời gian phát sinh nhỏ hơn hoặc bằng grace_period_minutes thì không tính block mới; nếu lớn hơn thì tính block mới. |
| Cash Rounding | Thanh toán tiền mặt làm tròn theo cash_rounding_unit và rounding_threshold. |
| Online Rounding | Thanh toán online không làm tròn. |
| Lost Card Penalty | Khi Staff xử lý Card bị mất, khách phải trả phí gửi xe hiện tại và `lost_card_penalty`; Card giữ trạng thái `LOST` cho đến khi Manager xử lý. |
| Wrong Zone Penalty | Xe đỗ sai khu vực có thể bị áp dụng wrong_zone_penalty khi được Staff xác nhận. |
| Configurable Variables Storage | Các biến giá tiền, timeout, grace period, penalty và rounding được cấu hình động ở tầng nghiệp vụ/application/admin configuration; không tạo thêm table riêng trong physical model hiện tại. |

---
# 8. Concept, Entity & Physical Model

## 8.1 Modeling Scope

Phần này đồng bộ SRS với concept relationship, entity list và physical table model. Mục tiêu là để nghiệp vụ, ERD và thiết kế database không bị tách rời.

Các quyết định đã chốt trong SRS được phản ánh trong model:

- Booking phải chọn `Building` trước.
- Booking xe máy và ô tô chỉ chọn `Building` và Vehicle/biển số; Driver không chọn `Zone` hoặc `Slot`.
- Actual Zone/Slot được hệ thống gán khi check-in và lưu ở `parking_session`.
- Ô tô vãng lai khi check-in chỉ được gợi ý `Zone`, không bắt buộc `Slot`.
- Ô tô Booking dùng Slot trong Zone `GENERAL` khi check-in; ô tô Monthly Subscription dùng Slot đã gán trong Zone `MONTHLY`.
- Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.
- Booking no-show dùng mốc 45 phút.
- Mỗi Monthly Subscription chỉ áp dụng cho một xe, không dùng `max_registered_plate`.
- Monthly Subscription có trạng thái `PENDING` khi chờ thanh toán/kích hoạt.
- Card có `card_type = NORMAL | MONTHLY`; Card `MONTHLY` được gán cho Monthly Subscription nhưng quyền lợi tháng vẫn do Monthly Subscription hợp lệ quyết định.
- Parking Session bắt buộc có `card_id` trong scope hiện tại; `monthly_subscription_id` là optional và không được dùng đồng thời với `booking_id`.
- Các biến pricing, timeout, grace period, penalty và rounding được cấu hình động ở tầng nghiệp vụ/application/admin configuration, không hard-code và không tạo thêm table riêng trong physical model hiện tại.

## 8.2 Entity Summary

| Entity/Table | Purpose |
|---|---|
| `role` | lưu vai trò của account. |
| `permission` | lưu quyền chức năng trong hệ thống. |
| `role_permission` | bảng trung gian cho quan hệ N-N giữa role và permission. |
| `account` | lưu tài khoản người dùng. Parking Staff được mô hình hóa bằng Account có Role phù hợp. |
| `building` | lưu tòa nhà gửi xe. |
| `floor` | lưu tầng thuộc tòa nhà. |
| `vehicle_type` | lưu loại phương tiện. |
| `zone` | lưu khu vực đỗ xe trong tầng. |
| `parking_slot` | lưu vị trí đỗ cụ thể, đặc biệt quan trọng với ô tô Walk-in/Booking trong Zone `GENERAL` và ô tô Monthly Subscription trong Zone `MONTHLY`. |
| `vehtypeicle` | lưu xe thuộc account. |
| `card` | lưu Card do bãi xe quản lý, gồm Card `NORMAL` cho Walk-in/Booking và Card `MONTHLY` gán dài hạn cho Monthly Subscription. |
| `parking_session` | lưu lượt gửi xe từ check-in đến check-out. |
| `incident_type` | lưu loại sự cố. |
| `incident` | lưu sự cố phát sinh trong session. |
| `blacklist` | lưu bản ghi chặn vehicle, card hoặc incident. |
| `booking` | lưu đặt chỗ trước. |
| `monthly_subscription` | lưu hồ sơ đăng ký và quyền lợi gửi xe định kỳ gắn với Vehicle, Building, Card `MONTHLY`, và Slot riêng nếu là ô tô. |
| `pricing_policy` | lưu chính sách giá theo loại xe. |
| `pricing_window` | lưu rule tính giá theo khung giờ. |
| `payment` | lưu giao dịch thanh toán từ parking session, booking hoặc monthly subscription. |
| `revenue_statistic` | lưu dữ liệu thống kê doanh thu. |
| `revenue_statistic_payment` | bảng nối để truy vết payment được aggregate vào revenue statistic. |
| `notification` | lưu thông báo gửi đến account. |
| `audit_log` | lưu log thao tác để truy vết. |

#### 8.3 Physical Model Normalized

> Mục tiêu: chỉnh sửa physical/logical model để AI CLI đọc được, bám theo relationship của `PBMS_Conceptual_Model.md`, đồng thời chuẩn hóa attribute/datatype từ các physical ERD đã cung cấp.

---

### 8.3.1 Modeling Rules

#### 8.3.1.1 Database-Agnostic Rule

File này không viết theo một database cụ thể.

Không dùng datatype đặc thù như:

- `nvarchar`
- `ntext`
- `bit`
- `boolean`
- `serial`
- `identity`

Các datatype trong tài liệu là kiểu trung lập để AI/Developer có thể map sang database cụ thể sau.

| Neutral Type | PostgreSQL Mapping | SQL Server Mapping | MySQL Mapping |
|---|---|---|---|
| `varchar(100)` | `varchar(100)` | `nvarchar(100)` nếu cần Unicode ở bước triển khai SQL Server | `varchar(100)` với charset UTF-8 |
| `int` | `integer` | `int` | `int` |
| `timestamp` | `timestamp` | `datetime2` | `datetime` |
| `decimal(18,2)` | `numeric(18,2)` | `decimal(18,2)` | `decimal(18,2)` |

---

#### 8.3.1.2 Naming Convention

| Item | Convention |
|---|---|
| Table name | `snake_case` |
| Column name | `snake_case` |
| Primary key | `<table_name>_id` hoặc tên ngắn đã rõ nghĩa |
| Foreign key | `<referenced_table>_id` |
| Status column | `varchar(20)` |
| Code column | `varchar(20)` hoặc `varchar(50)` |
| Name column | `varchar(50)` hoặc `varchar(100)` |
| Description / reason / message | `varchar(100)` |

---

#### 8.3.1.3 Allowed Data Types

| Data Type | Usage |
|---|---|
| `int` | ID, FK, number, count, flag 0/1 |
| `varchar(20)` | status, enum ngắn, short code, license plate, phone |
| `varchar(50)` | name ngắn, username, email ngắn, method, action |
| `varchar(100)` | description, reason, message, address, password hash, long name |
| `decimal(18,2)` | toàn bộ tiền, phí, doanh thu, số đo nếu cần |
| `date` | ngày |
| `time` | giờ trong ngày |
| `timestamp` | thời điểm đầy đủ ngày + giờ |

Quy ước chuẩn hóa:

- Tất cả số tiền dùng `decimal(18,2)`.
- Không tồn tại nhiều kiểu decimal khác nhau.
- Field `boolean`, `bit` trong ảnh được chuẩn hóa thành `int` với ý nghĩa `0/1`.
- Unicode được xử lý bằng database encoding, ví dụ UTF-8.

---

#### 8.3.1.4 Generic Constraints

| Constraint | Meaning |
|---|---|
| `PK` | Primary Key |
| `FK -> table.column` | Foreign Key |
| `NOT NULL` | Bắt buộc có dữ liệu |
| `NULL` | Cho phép rỗng |
| `UNIQUE` | Không được trùng |
| `AUTO GENERATED` | ID tự sinh / auto increase |
| `CHECK` | Ràng buộc nghiệp vụ logic |
| `DEFAULT` | Giá trị mặc định logic |

---

### 8.3.2 Relationship Summary From Conceptual Model

| ID | Relationship | Cardinality | Physical Direction |
|---|---|---|---|
| R-STR-001 | Parking Building has Floor | 1 - N | `floor.building_id -> building.building_id` |
| R-STR-002 | Floor has Zone | 1 - N | `zone.floor_id -> floor.floor_id` |
| R-STR-003 | Zone contains Parking Slot | 1 - 0..N | `parking_slot.zone_id -> zone.zone_id` |
| R-STR-004 | Vehicle Type classifies Parking Slot | 1 - 0..N | `parking_slot.vehicle_type_id -> vehicle_type.vehicle_type_id` |
| R-STR-005 | Vehicle Type classifies Vehicle | 1 - 0..N | `vehicle.vehicle_type_id -> vehicle_type.vehicle_type_id` |
| R-AUTH-001 | Role grants Permission | N - M | `role_permission(role_id, permission_id)` |
| R-AUTH-002 | Account assigned Role | N - 1 | `account.role_id -> role.role_id` |
| R-AUTH-003 | Account owns Vehicle | 1 - 0..N | `vehicle.account_id -> account.account_id` |
| R-AUTH-004 | Account receives Notification | 1 - 0..N | `notification.account_id -> account.account_id` |
| R-AUTH-005 | Account generates Audit Log | 1 - 0..N | `audit_log.account_id -> account.account_id` |
| R-OPS-001 | Vehicle has Parking Session | 1 - N | `parking_session.vehicle_id -> vehicle.vehicle_id` |
| R-OPS-002 | Card identifies Parking Session | 1 - N | `parking_session.card_id -> card.card_id` |
| R-OPS-003 | Parking Session occupies Parking Slot | N - 1 | `parking_session.slot_id -> parking_slot.slot_id` |
| R-OPS-004 | Parking Staff handles Parking Session | 0..N - N | `parking_session.in_staff_id/out_staff_id -> account.account_id` |
| R-OPS-005 | Parking Session has Incident | 1 - 0..N | `incident.session_id -> parking_session.session_id` |
| R-OPS-006 | Blacklist blocks Vehicle | 1 - 0..N | `blacklist.vehicle_id -> vehicle.vehicle_id` |
| R-OPS-007 | Blacklist blocks Card | 1 - 0..N | `blacklist.card_id -> card.card_id` |
| R-OPS-008 | Blacklist blocks Incident | 1 - 0..N | `blacklist.incident_id -> incident.incident_id` |
| R-BOOK-001 | Account has Booking | 1 - 0..N | `booking.account_id -> account.account_id` |
| R-BOOK-002 | Vehicle is used by Booking | 1 - 0..N | `booking.vehicle_id -> vehicle.vehicle_id` |
| R-BOOK-003 | Booking is requested in Building | Building 1 - 0..N Booking | `booking.building_id -> building.building_id` |
| R-BOOK-004 | Booking creates Payment | 1 - 0..N | `payment.booking_id -> booking.booking_id` |
| R-BOOK-005 | Booking converts to Parking Session | 0..1 - 1 | `parking_session.booking_id -> booking.booking_id` |
| R-MONTH-001 | Account subscribes Monthly Subscription | 1 - 0..N | `monthly_subscription.account_id -> account.account_id` |
| R-MONTH-002 | Vehicle registered in Monthly Subscription | 1 - 0..N | `monthly_subscription.vehicle_id -> vehicle.vehicle_id` |
| R-MONTH-003 | Monthly Subscription is assigned Card | Card 1 - 0..N Monthly Subscription | `monthly_subscription.assigned_card_id -> card.card_id` |
| R-MONTH-004 | Monthly Subscription includes Parking Slot for cars | Parking Slot 1 - 0..N Monthly Subscription | `monthly_subscription.assigned_slot_id -> parking_slot.slot_id` |
| R-MONTH-005 | Monthly Subscription creates Payment | 1 - 0..N | `payment.monthly_subscription_id -> monthly_subscription.monthly_subscription_id` |
| R-MONTH-006 | Monthly Subscription has Parking Session | 1 - 0..N | `parking_session.monthly_subscription_id -> monthly_subscription.monthly_subscription_id` |
| R-PAY-001 | Parking Session creates Payment | 1 - 0..N | `payment.session_id -> parking_session.session_id` |
| R-PAY-003 | Revenue Statistic aggregates Payment | 1 - N | `revenue_statistic_payment(statistic_id, payment_id)` |
| R-PRICE-001 | Vehicle Type applies Pricing Policy | 1 - 0..N | `pricing_policy.vehicle_type_id -> vehicle_type.vehicle_type_id` |
| R-PRICE-002 | Pricing Policy applies Payment | 1 - N | `payment.pricing_policy_id -> pricing_policy.pricing_policy_id` |
| R-PRICE-003 | Pricing Policy has Pricing Window | 1 - 1..N | `pricing_window.pricing_policy_id -> pricing_policy.pricing_policy_id` |

---

### 8.3.3 Physical Tables

#### 3.1 `role`

Purpose: lưu vai trò của account.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| role_id | int | PK, AUTO GENERATED, NOT NULL | ID vai trò |
| role_name | varchar(50) | NOT NULL, UNIQUE | Tên vai trò |
| description | varchar(100) | NULL | Mô tả vai trò |

---

#### 3.2 `permission`

Purpose: lưu quyền chức năng trong hệ thống.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| permission_id | int | PK, AUTO GENERATED, NOT NULL | ID quyền |
| permission_code | varchar(50) | NOT NULL, UNIQUE | Mã quyền |
| permission_name | varchar(50) | NOT NULL | Tên quyền |
| description | varchar(100) | NULL | Mô tả quyền |
| permission_status | varchar(20) | NOT NULL | Trạng thái quyền |

---

#### 3.3 `role_permission`

Purpose: bảng trung gian cho quan hệ N-N giữa role và permission.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| role_id | int | PK, FK -> role.role_id, NOT NULL | Vai trò |
| permission_id | int | PK, FK -> permission.permission_id, NOT NULL | Quyền |

---

#### 3.4 `account`

Purpose: lưu tài khoản người dùng. Parking Staff được mô hình hóa bằng Account có Role phù hợp.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| account_id | int | PK, AUTO GENERATED, NOT NULL | ID tài khoản |
| role_id | int | FK -> role.role_id, NOT NULL | Vai trò chính của tài khoản |
| username | varchar(50) | NOT NULL, UNIQUE | Tên đăng nhập |
| password_hash | varchar(100) | NOT NULL | Mật khẩu đã hash |
| full_name | varchar(100) | NULL | Họ tên |
| email | varchar(100) | NULL, UNIQUE | Email |
| phone | varchar(20) | NULL | Số điện thoại |
| account_status | varchar(20) | NOT NULL | Trạng thái tài khoản |
| created_at | timestamp | NOT NULL | Thời điểm tạo |

---

#### 3.5 `building`

Purpose: lưu tòa nhà gửi xe.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| building_id | int | PK, AUTO GENERATED, NOT NULL | ID tòa nhà |
| building_name | varchar(50) | NOT NULL | Tên tòa nhà |
| address | varchar(100) | NULL | Địa chỉ |
| total_floor | int | NOT NULL | Tổng số tầng |
| building_status | varchar(20) | NOT NULL | Trạng thái tòa nhà |
| created_at | timestamp | NOT NULL | Thời điểm tạo |

---

#### 3.6 `floor`

Purpose: lưu tầng thuộc tòa nhà.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| floor_id | int | PK, AUTO GENERATED, NOT NULL | ID tầng |
| building_id | int | FK -> building.building_id, NOT NULL | Tòa nhà chứa tầng |
| floor_number | int | NOT NULL | Số tầng |
| floor_name | varchar(50) | NULL | Tên tầng |
| floor_status | varchar(20) | NOT NULL | Trạng thái tầng |

Generic constraints:

- `UNIQUE(building_id, floor_number)`

---

#### 3.7 `zone`

Purpose: lưu khu vực đỗ xe trong tầng.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| zone_id | int | PK, AUTO GENERATED, NOT NULL | ID zone |
| floor_id | int | FK -> floor.floor_id, NOT NULL | Tầng chứa zone |
| vehicle_type_id | int | FK -> vehicle_type.vehicle_type_id, NOT NULL | Loại xe được phục vụ |
| zone_code | varchar(20) | NOT NULL | Mã zone |
| zone_name | varchar(50) | NOT NULL | Tên zone |
| capacity | int | NOT NULL | Sức chứa zone |
| zone_access_type | varchar(20) | NOT NULL, DEFAULT `GENERAL` | GENERAL, MONTHLY |
| zone_status | varchar(20) | NOT NULL | Trạng thái zone |

Generic constraints:

- `UNIQUE(floor_id, zone_code)`
- `CHECK(capacity >= 0)`
- `zone_access_type` dùng để tách Zone ô tô `GENERAL` cho Walk-in/Booking và `MONTHLY` cho Monthly Subscription.
- Motorcycle monthly capacity không yêu cầu Zone `MONTHLY`; mỗi subscription active giữ một đơn vị capacity động ở Building.

---

#### 3.8 `parking_slot`

Purpose: lưu vị trí đỗ cụ thể, đặc biệt quan trọng với ô tô booking và ô tô thẻ tháng.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| slot_id | int | PK, AUTO GENERATED, NOT NULL | ID slot |
| zone_id | int | FK -> zone.zone_id, NOT NULL | Zone chứa slot |
| vehicle_type_id | int | FK -> vehicle_type.vehicle_type_id, NOT NULL | Loại xe phù hợp |
| slot_code | varchar(20) | NOT NULL, UNIQUE | Mã slot |
| slot_name | varchar(50) | NULL | Tên hiển thị |
| slot_status | varchar(20) | NOT NULL | AVAILABLE, OCCUPIED, BLOCKED, MAINTENANCE |
| created_at | timestamp | NOT NULL | Thời điểm tạo |

Generic constraints:

- Một slot không được có nhiều active parking session cùng lúc.
- Ô tô Walk-in/Booking chỉ được gán Slot trong Zone `GENERAL`.
- Ô tô thẻ tháng dùng Slot trong Zone `MONTHLY` thông qua `monthly_subscription.assigned_slot_id`.
- Không dùng `slot_status` để biểu diễn Slot đã gán dài hạn cho Monthly Subscription.

---

#### 3.9 `vehicle_type`

Purpose: lưu loại phương tiện.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| vehicle_type_id | int | PK, AUTO GENERATED, NOT NULL | ID loại xe |
| type_name | varchar(50) | NOT NULL, UNIQUE | Tên loại xe |
| description | varchar(100) | NULL | Mô tả |
| vehicle_type_status | varchar(20) | NOT NULL | Trạng thái loại xe |

---

#### 3.10 `vehicle`

Purpose: lưu xe thuộc account.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| vehicle_id | int | PK, AUTO GENERATED, NOT NULL | ID xe |
| account_id | int | FK -> account.account_id, NULL | Chủ xe; nullable vì có thể nhập xe trước hoặc khách vãng lai |
| vehicle_type_id | int | FK -> vehicle_type.vehicle_type_id, NOT NULL | Loại xe |
| license_plate | varchar(20) | NOT NULL, UNIQUE | Biển số xe |
| registered_day | date | NULL | Ngày đăng ký xe trong hệ thống |
| vehicle_status | varchar(20) | NOT NULL | Trạng thái xe trên hệ thống |

---

#### 3.11 `card`

Purpose: lưu Card do bãi xe quản lý. Card `NORMAL` được cấp tạm thời cho Walk-in/Booking; Card `MONTHLY` được gán dài hạn cho Monthly Subscription và Driver giữ trong thời hạn quyền lợi.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| card_id | int | PK, AUTO GENERATED, NOT NULL | ID card |
| card_code | varchar(20) | NOT NULL, UNIQUE | Mã nghiệp vụ để Staff nhập thủ công, ví dụ `CARD-000001` |
| nfc_uid | varchar(50) | NULL, UNIQUE WHEN NOT NULL | UID của chip NFC nếu Card đã được gắn NFC |
| card_type | varchar(20) | NOT NULL | NORMAL, MONTHLY |
| card_status | varchar(20) | NOT NULL, DEFAULT `AVAILABLE` | AVAILABLE, ASSIGNED, LOST, BLOCKED |
| created_at | timestamp | NOT NULL | Thời điểm tạo Card |
| updated_at | timestamp | NULL | Thời điểm cập nhật cuối |

Generic constraints:

- `card_code` phải được sinh tự động theo format thống nhất, ví dụ `CARD-000001`, và không dùng `COUNT + 1` khi có nguy cơ trùng do concurrent request.
- `nfc_uid` được phép `NULL`; khi có giá trị thì phải duy nhất.
- Một Card chỉ được gắn với tối đa một Parking Session `ACTIVE`.
- Card `NORMAL` chỉ dùng cho Walk-in/Booking, chuyển `AVAILABLE -> ASSIGNED -> AVAILABLE`.
- Card `MONTHLY` được gán qua `monthly_subscription.assigned_card_id`, giữ trạng thái `ASSIGNED` qua nhiều lượt check-in/check-out.
- Card type không tự cấp quyền lợi Monthly Subscription; quyền lợi phụ thuộc vào Monthly Subscription hợp lệ.

---

#### 3.12 `parking_session`

Purpose: lưu lượt gửi xe từ check-in đến check-out.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| session_id | int | PK, AUTO GENERATED, NOT NULL | ID session |
| vehicle_id | int | FK -> vehicle.vehicle_id, NOT NULL | Xe gửi |
| building_id | int | FK -> building.building_id, NOT NULL | Building nơi session diễn ra |
| card_id | int | FK -> card.card_id, NOT NULL | Card được dùng để nhận diện session |
| zone_id | int | FK -> zone.zone_id, NULL | Zone được gợi ý hoặc sử dụng |
| slot_id | int | FK -> parking_slot.slot_id, NULL | Slot được dùng nếu có |
| booking_id | int | FK -> booking.booking_id, NULL, UNIQUE | Booking chuyển thành session |
| monthly_subscription_id | int | FK -> monthly_subscription.monthly_subscription_id, NULL | Quyền lợi Monthly Subscription được áp dụng nếu có |
| in_staff_id | int | FK -> account.account_id, NULL | Staff xử lý check-in |
| out_staff_id | int | FK -> account.account_id, NULL | Staff xử lý check-out |
| check_in_time | timestamp | NOT NULL | Thời điểm vào |
| check_out_time | timestamp | NULL | Thời điểm ra |
| license_plate_in | varchar(20) | NOT NULL | Biển số lúc vào |
| license_plate_out | varchar(20) | NULL | Biển số lúc ra |
| session_status | varchar(20) | NOT NULL | Trạng thái vòng đời lượt gửi xe |

Generic constraints:

- Một `vehicle_id` chỉ được có tối đa một session `ACTIVE` cùng lúc.
- Một `card_id` chỉ được có tối đa một session `ACTIVE` cùng lúc.
- Một `slot_id` chỉ được có tối đa một session `ACTIVE` cùng lúc.
- `booking_id` unique vì một booking chỉ được chuyển thành tối đa một session.
- `booking_id` và `monthly_subscription_id` không được đồng thời có giá trị.
- Xe máy có thể `slot_id = NULL` và dùng `zone_id`.
- Ô tô Walk-in/Booking phải dùng `slot_id` thuộc Zone `GENERAL`.
- Ô tô Monthly Subscription phải dùng `slot_id = monthly_subscription.assigned_slot_id` thuộc Zone `MONTHLY`.
- Monthly Subscription chỉ áp dụng khi `parking_session.building_id = monthly_subscription.building_id`.

---

#### 3.13 `incident_type`

Purpose: lưu loại sự cố.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| incident_type_id | int | PK, AUTO GENERATED, NOT NULL | ID loại sự cố |
| incident_code | varchar(20) | NOT NULL, UNIQUE | Mã loại sự cố |
| incident_name | varchar(50) | NOT NULL | Tên loại sự cố |
| description | varchar(100) | NULL | Mô tả |
| default_penalty_fee | decimal(18,2) | NULL | Phí phạt mặc định |

---

#### 3.14 `incident`

Purpose: lưu sự cố phát sinh trong session.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| incident_id | int | PK, AUTO GENERATED, NOT NULL | ID sự cố |
| session_id | int | FK -> parking_session.session_id, NOT NULL | Session phát sinh sự cố |
| incident_type_id | int | FK -> incident_type.incident_type_id, NOT NULL | Loại sự cố |
| description | varchar(100) | NULL | Mô tả |
| penalty_fee | decimal(18,2) | NULL | Phí phạt |
| incident_status | varchar(20) | NOT NULL | Trạng thái incident |
| created_at | timestamp | NOT NULL | Thời điểm tạo |
| resolved_at | timestamp | NULL | Thời điểm xử lý xong |

---

#### 3.15 `blacklist`

Purpose: lưu bản ghi chặn vehicle, card hoặc incident.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| blacklist_id | int | PK, AUTO GENERATED, NOT NULL | ID blacklist |
| vehicle_id | int | FK -> vehicle.vehicle_id, NULL | Xe bị chặn |
| card_id | int | FK -> card.card_id, NULL | Card bị chặn |
| incident_id | int | FK -> incident.incident_id, NULL | Sự cố dẫn tới blacklist |
| reason | varchar(100) | NOT NULL | Lý do chặn |
| created_at | timestamp | NOT NULL | Thời điểm tạo |

Generic constraints:

- `CHECK(vehicle_id IS NOT NULL OR card_id IS NOT NULL OR incident_id IS NOT NULL)`

---

#### 3.16 `booking`

Purpose: lưu đặt chỗ trước.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| booking_id | int | PK, AUTO GENERATED, NOT NULL | ID booking |
| account_id | int | FK -> account.account_id, NOT NULL | Người tạo booking |
| vehicle_id | int | FK -> vehicle.vehicle_id, NOT NULL | Xe được booking |
| vehicle_type_id | int | FK -> vehicle_type.vehicle_type_id, NOT NULL | Loại xe tại thời điểm booking |
| building_id | int | FK -> building.building_id, NOT NULL | Tòa nhà booking |
| planned_checkin_time | timestamp | NOT NULL | Giờ dự kiến vào |
| planned_checkout_time | timestamp | NOT NULL | Giờ dự kiến ra |
| deposit_amount | decimal(18,2) | NOT NULL | Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành |
| booking_status | varchar(20) | NOT NULL | Trạng thái quy trình booking. |
| payment_deadline | timestamp | NOT NULL | Hạn thanh toán cọc |
| checkin_grace_until | timestamp | NOT NULL | Hạn check-in sau grace time |
| cancelled_at | timestamp | NULL | Thời điểm hủy |
| cancel_reason | varchar(100) | NULL | Lý do hủy |
| confirmed_at | timestamp | NULL | Thời điểm xác nhận |
| created_at | timestamp | NOT NULL | Thời điểm tạo |

Generic constraints:

- Booking phải có `building_id`, Vehicle/biển số và planned time.
- Booking không lưu Driver-selected `zone_id` hoặc `slot_id`.
- Xe máy booking giữ một general capacity unit tại Building; actual Zone được lưu ở `parking_session.zone_id` khi check-in.
- Ô tô booking giữ general car capacity tại Building; actual Slot trong Zone `GENERAL` được lưu ở `parking_session.slot_id` khi check-in.
- Với booking ô tô đã `CONFIRMED`, Slot chưa nằm trong Booking; hệ thống xác định Slot trong Zone `GENERAL` khi check-in.
- `planned_checkout_time` phải sau `planned_checkin_time`.
- Một booking chỉ được chuyển thành tối đa một parking session.
- Nếu hết `payment_deadline` mà chưa thanh toán, booking bị hủy.
- Nếu quá `checkin_grace_until` mà chưa check-in, booking hết hạn/no-show.

---

#### 3.17 `monthly_subscription`

Purpose: lưu hồ sơ đăng ký gửi xe định kỳ, chu kỳ hiệu lực và quyền lợi tháng của một Vehicle trên hệ thống; không đại diện cho Card vật lý hoặc dữ liệu được lưu trong chip NFC.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| monthly_subscription_id | int | PK, AUTO GENERATED, NOT NULL | ID hồ sơ đăng ký gửi xe tháng |
| account_id | int | FK -> account.account_id, NOT NULL | Người đăng ký |
| vehicle_id | int | FK -> vehicle.vehicle_id, NOT NULL | Xe được áp dụng quyền lợi |
| assigned_card_id | int | FK -> card.card_id, NULL, UNIQUE | Card `MONTHLY` cấp cho Driver |
| assigned_slot_id | int | FK -> parking_slot.slot_id, NULL | Slot riêng của ô tô Monthly Subscription |
| building_id | int | FK -> building.building_id, NOT NULL | Tòa nhà áp dụng |
| monthly_price | decimal(18,2) | NOT NULL | Giá tại thời điểm đăng ký |
| activated_at | timestamp | NULL | Thời điểm kích hoạt sau khi thanh toán thành công |
| expired_at | timestamp | NULL | Thời điểm hết hiệu lực |
| monthly_subscription_status | varchar(20) | NOT NULL, DEFAULT `PENDING` | PENDING, ACTIVE, EXPIRED, DOWNGRADED, CANCELLED |
| created_at | timestamp | NOT NULL | Thời điểm tạo hồ sơ |

Generic constraints:

- `monthly_price >= 0`.
- Nếu có cả `activated_at` và `expired_at` thì `expired_at > activated_at`.
- Xe máy Monthly Subscription phải có `assigned_slot_id = NULL`.
- Ô tô Monthly Subscription phải có `assigned_slot_id` trước khi quyền lợi được kích hoạt.
- `assigned_card_id` phải trỏ tới Card có `card_type = MONTHLY` và `card_status = ASSIGNED` khi subscription active.
- Slot ô tô Monthly Subscription phải thuộc cùng Building thông qua `assigned_slot -> zone -> floor -> building`.
- Slot ô tô Monthly Subscription phải thuộc Zone có `zone_access_type = MONTHLY`.
- Mỗi Monthly Subscription chỉ áp dụng cho một `vehicle_id`.
- Một xe không được có nhiều Monthly Subscription `ACTIVE` trùng thời gian hiệu lực.
- Monthly Subscription ở trạng thái `PENDING` khi chờ thanh toán/kích hoạt; khi đó `activated_at` và `expired_at` có thể `NULL`.
- Card `MONTHLY` có thể được giữ bởi Driver qua nhiều lượt check-in/check-out; Card type không thay thế validation Monthly Subscription.

---

#### 3.18 `pricing_policy`

Purpose: lưu chính sách giá theo loại xe.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| pricing_policy_id | int | PK, AUTO GENERATED, NOT NULL | ID chính sách giá |
| vehicle_type_id | int | FK -> vehicle_type.vehicle_type_id, NOT NULL | Loại xe áp dụng |
| policy_name | varchar(100) | NOT NULL | Tên chính sách |
| effective_start | date | NOT NULL | Ngày bắt đầu hiệu lực |
| effective_end | date | NULL | Ngày hết hiệu lực |
| pricing_policy_status | varchar(20) | NOT NULL | Trạng thái chính sách |

Generic constraints:

- `effective_end` phải null hoặc sau `effective_start`.
- Mỗi policy có ít nhất một pricing window.

---

#### 3.19 `pricing_window`

Purpose: lưu rule tính giá theo khung giờ.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| pricing_window_id | int | PK, AUTO GENERATED, NOT NULL | ID pricing window |
| pricing_policy_id | int | FK -> pricing_policy.pricing_policy_id, NOT NULL | Chính sách giá cha |
| window_name | varchar(50) | NOT NULL | Tên khung giờ |
| start_time | time | NOT NULL | Giờ bắt đầu |
| end_time | time | NOT NULL | Giờ kết thúc |
| base_duration_minutes | int | NOT NULL | Thời lượng cơ bản |
| base_price | decimal(18,2) | NOT NULL | Giá cơ bản |
| increment_block_minutes | int | NOT NULL | Kích thước block phát sinh |
| increment_price | decimal(18,2) | NOT NULL | Giá mỗi block phát sinh |
| window_cap | decimal(18,2) | NULL | Mức giá tối đa của window |
| grace_period_minutes | int | NOT NULL, DEFAULT 0 | Thời gian ân hạn |

Generic constraints:

- `base_duration_minutes > 0`
- `increment_block_minutes > 0`
- `base_price >= 0`
- `increment_price >= 0`
- `window_cap` null hoặc `window_cap >= base_price`
- Window cap chỉ áp dụng trong từng pricing window, không áp dụng toàn session.

---

#### 3.20 `payment`

Purpose: lưu giao dịch thanh toán từ parking session, booking hoặc monthly subscription.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| payment_id | int | PK, AUTO GENERATED, NOT NULL | ID payment |
| session_id | int | FK -> parking_session.session_id, NULL | Payment từ session |
| booking_id | int | FK -> booking.booking_id, NULL | Payment từ booking |
| monthly_subscription_id | int | FK -> monthly_subscription.monthly_subscription_id, NULL | Payment từ monthly subscription |
| pricing_policy_id | int | FK -> pricing_policy.pricing_policy_id, NULL | Chính sách giá dùng để tính |
| amount | decimal(18,2) | NOT NULL | Số tiền |
| payment_method | varchar(20) | NOT NULL | CASH, ONLINE_BANKING |
| payment_time | timestamp | NULL | Thời điểm thanh toán |
| payment_status | varchar(20) | NOT NULL | PENDING, PAID, FAILED, REFUNDED |
| created_at | timestamp | NOT NULL | Thời điểm tạo payment |

Generic constraints:

- `CHECK(session_id IS NOT NULL OR booking_id IS NOT NULL OR monthly_subscription_id IS NOT NULL)`
- Payment source phải truy vết được.
- Payment nên lưu pricing policy hoặc fee detail để audit khi bảng giá thay đổi.

---

#### 3.21 `revenue_statistic`

Purpose: lưu dữ liệu thống kê doanh thu.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| statistic_id | int | PK, AUTO GENERATED, NOT NULL | ID thống kê |
| stat_date | date | NOT NULL | Ngày thống kê |
| vehicle_type_id | int | FK -> vehicle_type.vehicle_type_id, NULL | Loại xe nếu thống kê theo loại |
| payment_method | varchar(20) | NULL | Phương thức thanh toán dùng như dimension thống kê |
| total_payments_count | int | NOT NULL | Tổng số payment trong nhóm thống kê |
| total_revenue | decimal(18,2) | NOT NULL | Tổng doanh thu |
| updated_at | timestamp | NOT NULL | Thời điểm cập nhật |

Generic constraints:

- `payment_method` là giá trị grouping/snapshot từ `payment.payment_method`, không phải FK vì `payment_method` không phải entity riêng.
- Nếu cần truy vết payment nào nằm trong statistic nào, dùng bảng nối `revenue_statistic_payment`.
- Statistic có thể được tạo từ query, job tổng hợp hoặc materialized data.

---

#### 3.22 `revenue_statistic_payment`

Purpose: bảng nối để truy vết payment được aggregate vào revenue statistic.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| statistic_id | int | PK, FK -> revenue_statistic.statistic_id, NOT NULL | Dòng thống kê |
| payment_id | int | PK, FK -> payment.payment_id, NOT NULL | Payment được tổng hợp |

Generic constraints:

- Một `revenue_statistic` có thể aggregate nhiều `payment`.
- Một `payment` có thể xuất hiện trong nhiều statistic khác nhau nếu hệ thống tạo nhiều kiểu thống kê, ví dụ theo ngày, theo loại xe, theo phương thức thanh toán.

---

#### 3.23 `notification`

Purpose: lưu thông báo gửi đến account.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| notification_id | int | PK, AUTO GENERATED, NOT NULL | ID thông báo |
| account_id | int | FK -> account.account_id, NOT NULL | Người nhận |
| title | varchar(100) | NOT NULL | Tiêu đề |
| message | varchar(100) | NOT NULL | Nội dung |

---

#### 3.24 `audit_log`

Purpose: lưu log thao tác để truy vết.

| Column | Type | Constraints | Meaning |
|---|---|---|---|
| audit_log_id | int | PK, AUTO GENERATED, NOT NULL | ID log |
| account_id | int | FK -> account.account_id, NULL | Người thực hiện |
| action | varchar(50) | NOT NULL | Hành động |
| target_table | varchar(50) | NULL | Bảng/entity bị tác động |
| target_id | int | NULL | ID bản ghi bị tác động |
| description | varchar(100) | NULL | Mô tả |
| created_at | timestamp | NOT NULL | Thời điểm ghi log |

---

### 8.3.4 Mermaid ERD With Physical Tables

```mermaid
erDiagram
    BUILDING ||--o{ FLOOR : has
    FLOOR ||--o{ ZONE : has
    VEHICLE_TYPE ||--o{ ZONE : classifies
    ZONE ||--o{ PARKING_SLOT : contains
    VEHICLE_TYPE ||--o{ PARKING_SLOT : classifies

    ROLE ||--o{ ACCOUNT : assigned
    ROLE ||--o{ ROLE_PERMISSION : has
    PERMISSION ||--o{ ROLE_PERMISSION : granted
    ACCOUNT ||--o{ VEHICLE : owns
    ACCOUNT ||--o{ NOTIFICATION : receives
    ACCOUNT ||--o{ AUDIT_LOG : generates

    VEHICLE_TYPE ||--o{ VEHICLE : classifies
    VEHICLE ||--o{ PARKING_SESSION : has
    CARD ||--o{ PARKING_SESSION : identifies
    ZONE ||--o{ PARKING_SESSION : used_by
    PARKING_SLOT ||--o{ PARKING_SESSION : occupied_by
    ACCOUNT ||--o{ PARKING_SESSION : checkin_staff
    ACCOUNT ||--o{ PARKING_SESSION : checkout_staff

    PARKING_SESSION ||--o{ INCIDENT : has
    INCIDENT_TYPE ||--o{ INCIDENT : classifies
    VEHICLE ||--o{ BLACKLIST : blocked_by
    CARD ||--o{ BLACKLIST : blocked_by
    INCIDENT ||--o{ BLACKLIST : caused_by

    ACCOUNT ||--o{ BOOKING : creates
    VEHICLE ||--o{ BOOKING : used_for
    BUILDING ||--o{ BOOKING : requested_in
    BOOKING ||--o| PARKING_SESSION : converts_to

    ACCOUNT ||--o{ MONTHLY_SUBSCRIPTION : subscribes
    VEHICLE ||--o{ MONTHLY_SUBSCRIPTION : registered_for
    BUILDING ||--o{ MONTHLY_SUBSCRIPTION : applies_in
    CARD ||--o{ MONTHLY_SUBSCRIPTION : assigned_to
    PARKING_SLOT ||--o{ MONTHLY_SUBSCRIPTION : assigned_to
    MONTHLY_SUBSCRIPTION ||--o{ PARKING_SESSION : used_by

    VEHICLE_TYPE ||--o{ PRICING_POLICY : applies
    PRICING_POLICY ||--o{ PRICING_WINDOW : has
    PRICING_POLICY ||--o{ PAYMENT : used_by

    PARKING_SESSION ||--o{ PAYMENT : creates
    BOOKING ||--o{ PAYMENT : creates
    MONTHLY_SUBSCRIPTION ||--o{ PAYMENT : creates

    VEHICLE_TYPE ||--o{ REVENUE_STATISTIC : grouped_by
    REVENUE_STATISTIC ||--o{ REVENUE_STATISTIC_PAYMENT : contains
    PAYMENT ||--o{ REVENUE_STATISTIC_PAYMENT : included_in
```

---

### 8.3.5 Cross-Domain Constraints

| Constraint ID | Constraint |
|---|---|
| C-001 | Một Vehicle không được có nhiều Parking Session `ACTIVE` cùng lúc. |
| C-002 | Một Parking Slot không được có nhiều Parking Session `ACTIVE` cùng lúc. |
| C-003 | Booking phải có `building_id`, Vehicle/biển số và planned time; Booking không lưu Driver-selected Zone/Slot. |
| C-004 | Booking xe máy giữ general capacity ở Building; actual Zone được lưu ở Parking Session khi check-in. |
| C-005 | Booking ô tô giữ general capacity ở Building; actual Slot trong Zone `GENERAL` được lưu ở Parking Session khi check-in. |
| C-006 | Monthly Subscription ô tô phải có `assigned_slot_id` trong Zone `MONTHLY` trước khi kích hoạt. |
| C-007 | Monthly Subscription xe máy phải có `assigned_slot_id = NULL` và giữ capacity động tại Building. |
| C-008 | Mỗi Monthly Subscription chỉ áp dụng cho một `vehicle_id`. |
| C-009 | Một Booking chỉ được chuyển thành tối đa một Parking Session. |
| C-010 | Payment phải truy vết được nguồn: session, booking hoặc monthly subscription. |
| C-011 | Payment nên lưu pricing policy để audit khi bảng giá thay đổi. |
| C-012 | Revenue Statistic là dữ liệu tổng hợp; nếu cần trace chi tiết thì dùng `revenue_statistic_payment`. |
| C-013 | Card và Vehicle có thể bị blacklist, nhưng nguồn sự kiện nên truy vết qua Incident nếu có. |
| C-014 | Staff được triển khai bằng Account có Role phù hợp, không bắt buộc tạo table Parking Staff riêng. |
| C-015 | Các biến pricing/timeout/grace/penalty/rounding được cấu hình động ở tầng nghiệp vụ/application/admin configuration, không tạo thêm table riêng trong physical model hiện tại. |
| C-016 | Parking Session trong scope hiện tại bắt buộc có `card_id`. |
| C-017 | Parking Session không được đồng thời có cả `booking_id` và `monthly_subscription_id`. |
| C-018 | Card `MONTHLY` có thể gán với một active Monthly Subscription qua `monthly_subscription.assigned_card_id`; Card `NORMAL` không được gán vào trường này. |
| C-019 | Slot status không được dùng để biểu diễn quyền giữ Slot tháng; quyền giữ Slot tháng nằm ở `monthly_subscription.assigned_slot_id`. |

---

### 8.3.6 Notes / Deviations From Previous Version

| Area | Previous Version | Updated Decision |
|---|---|---|
| String type | Có database-specific Unicode type | Bỏ toàn bộ, dùng `varchar(100)`. |
| Building table | `parking_building` | Đổi thành `building`. |
| Role | Có `role_status` | Bỏ `role_status`. |
| Vehicle Type | Có `width_limit`, `height_limit` | Bỏ hai field này. |
| Vehicle | Có `color` | Bỏ `color`. |
| Incident Type | Có `incident_type_status` | Bỏ status khỏi `incident_type`; status nằm ở `incident.incident_status`. |
| Invoice | Có entity `invoice` | Bỏ entity `invoice`. |
| Report | Có entity `report` | Bỏ entity `report`. |
| Revenue Statistic | Không có FK trace payment chi tiết | Thêm bảng nối `revenue_statistic_payment`. |
| Notification | Có thời điểm đọc/tạo và có thể có type | Bỏ các field đó; chỉ giữ nội dung và trạng thái. |
| Booking allocation | Ô tô booking bắt buộc chọn Slot ngay từ đầu | Booking chỉ chọn Building và Vehicle/biển số; Zone/Slot thực tế do hệ thống gán khi check-in. |
| Monthly Subscription naming | Table/entity cũ dùng tên thẻ tháng | Đổi tên kỹ thuật thành `monthly_subscription`; nghiệp vụ tiếng Việt vẫn có thể gọi là thẻ tháng/vé tháng. |
| Monthly Subscription vehicle count | Có `max_registered_plate` | Bỏ `max_registered_plate`; mỗi Monthly Subscription chỉ áp dụng cho một xe. |
| Card identifier | Có định danh RFID và bỏ phân loại Card | Dùng `nfc_uid` và giữ `card_type = NORMAL, MONTHLY`; Card `MONTHLY` được gán với Monthly Subscription nhưng không tự cấp quyền lợi. |
| Parking Session card | `parking_session.card_id` nullable | Trong scope hiện tại `parking_session.card_id` là bắt buộc. |
| Config table | Có thể tạo bảng cấu hình riêng | Không tạo thêm table; biến cấu hình được quản lý động ở tầng nghiệp vụ/application/admin configuration. |
| Parking Slot | Có flag is_reserved để truy xuất nhanh | Bỏ is_reserved để tránh mâu thuẫn với slot_status. |
| Vehicle & Card | Có flag is_blacklisted để truy xuất nhanh | Bỏ is_blacklisted để tránh mâu thuẫn với trạng thái blacklist đã được lưu bằng bảng blacklist. |

---

### 8.3.7 Verification Checklist

| Check | Expected |
|---|---|
| Database-specific Unicode type    | Không còn dùng datatype đặc thù như `nvarchar`; toàn bộ text dùng `varchar. |
| Building table                    | Dùng table `building`, không dùng `parking_building. |
| Role                              | Không còn `role_status. |
| Vehicle Type                      | Không còn `width_limit`, `height_limit. |
| Vehicle                           | Không còn `color`; không còn `is_blacklisted. |
| Card                              | Không còn `is_blacklisted. |
| Parking Slot                      | Không còn `is_reserved`; `slot_status` chỉ gồm trạng thái vật lý/vận hành như AVAILABLE, OCCUPIED, BLOCKED, MAINTENANCE. |
| Incident Type                     | Không còn `incident_type_status. |
| Incident                          | Có `incident_status. |
| Invoice                           | Không còn table `invoice. |
| Report                            | Không còn table `report. |
| Blacklist                         | Bảng `blacklist` là nguồn chính để xác định vehicle/card/incident bị chặn. |
| Revenue Statistic                 | Có `revenue_statistic_payment` để trace payment được aggregate vào statistic. |
| Notification                      | Không có thời điểm đọc/tạo/type. |
| Payment Type                      | Nên có `payment_type` và `payment.payment_type_id` để phân biệt `PARKING_FEE`, `BOOKING_DEPOSIT`, `MONTHLY_SUBSCRIPTION_PURCHASE`, `PENALTY`, `REFUND`, `ADJUSTMENT`.                                             |
| Payment source                    | `payment` có thể link đến session, booking hoặc monthly subscription, nhưng mỗi payment nên có đúng một nguồn nghiệp vụ chính. |
| Pricing                           | `pricing_policy` link `pricing_window. |
| Booking allocation                | `booking.building_id` bắt buộc; Booking không có `zone_id` hoặc `slot_id`; actual location nằm ở Parking Session. |
| Booking location consistency      | Motorcycle Booking check-in lưu actual Zone; car Booking check-in lưu actual Slot trong Zone `GENERAL` cùng Building. |
| Monthly subscription slot         | `monthly_subscription.assigned_slot_id` nullable cho xe máy nhưng required cho ô tô trước khi kích hoạt; Slot ô tô phải thuộc Zone `MONTHLY`. |
| Monthly subscription slot uniqueness | Không được có hai Monthly Subscription `ACTIVE` dùng cùng một `assigned_slot_id` trong cùng thời gian hiệu lực. |
| Monthly subscription vehicle count | Không còn `max_registered_plate`; mỗi Monthly Subscription gắn một `vehicle_id`. |
| Monthly subscription pending dates | `monthly_subscription.activated_at` và `expired_at` được phép `NULL` khi status là `PENDING`. |
| Card model                        | Có `card_type = NORMAL, MONTHLY`; dùng `nfc_uid` nullable và unique khi có giá trị; Card `MONTHLY` liên kết qua `monthly_subscription.assigned_card_id`. |
| Parking session active constraint | Một vehicle không được có nhiều parking session `ACTIVE` cùng lúc. |
| Slot active constraint            | Một parking slot không được có nhiều parking session `ACTIVE` cùng lúc. |
| Config table                      | Không có table cấu hình riêng cho pricing/timeout/grace/penalty/rounding trong phiên bản hiện tại. |

---
