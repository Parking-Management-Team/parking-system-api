using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Configurations;

/// <summary>
/// Cấu hình bảng "card" trong database sử dụng Fluent API của EF Core.
///
/// Bảng này lưu thông tin mã thẻ gửi xe mô phỏng (thay thế thẻ vật lý thật).
/// Tham chiếu SRS: §8.3.3, Table 3.11 — Physical Model: card
///
/// Ràng buộc quan trọng từ SRS:
///   - card_code : UNIQUE, NOT NULL, varchar(20)
///   - rfid_code : UNIQUE khi có giá trị, NULL cho phép, varchar(50)
///   - card_type : NOT NULL, varchar(20)
///   - card_status: NOT NULL, varchar(20), default = "Available"
/// </summary>
public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        // 1. Ánh xạ với tên bảng vật lý "card" trong database (theo SRS naming: snake_case)
        builder.ToTable("card");

        // 2. Khóa chính — Id kế thừa từ BaseEntity, ánh xạ thành cột "card_id"
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("card_id")
            .ValueGeneratedOnAdd(); // PostgreSQL: SERIAL / GENERATED ALWAYS AS IDENTITY

        // 3. Mã thẻ nội bộ (CardCode) — bắt buộc, tối đa 20 ký tự, UNIQUE
        // Đây là mã in ra vé/QR cho khách cầm khi gửi xe
        builder.Property(c => c.CardCode)
            .HasColumnName("card_code")
            .HasMaxLength(20)
            .IsRequired();

        // Tạo Unique Index để đảm bảo không có 2 thẻ trùng mã trong hệ thống
        builder.HasIndex(c => c.CardCode)
            .IsUnique()
            .HasDatabaseName("IX_card_card_code");

        // 4. Mã RFID mô phỏng (RfidCode) — tuỳ chọn, tối đa 50 ký tự, UNIQUE khi có giá trị
        builder.Property(c => c.RfidCode)
            .HasColumnName("rfid_code")
            .HasMaxLength(50)
            .IsRequired(false); // NULL cho phép

        // Unique Index cho RfidCode — chỉ áp dụng khi giá trị khác NULL (Filtered Index)
        builder.HasIndex(c => c.RfidCode)
            .IsUnique()
            .HasFilter("rfid_code IS NOT NULL") // PostgreSQL: bỏ qua các bản ghi NULL
            .HasDatabaseName("IX_card_rfid_code");

        // 5. Loại thẻ (CardType) — bắt buộc, tối đa 20 ký tự
        // Ví dụ giá trị: "PARKING_CARD", "ACCESS_CARD"
        builder.Property(c => c.CardType)
            .HasColumnName("card_type")
            .HasMaxLength(20)
            .IsRequired();

        // 6. Trạng thái thẻ (CardStatus) — bắt buộc, tối đa 20 ký tự
        // Các giá trị hợp lệ: "Available", "Active", "Lost", "Blocked" (theo enum CardStatus)
        // Mặc định khi tạo mới: "Available"
        builder.Property(c => c.CardStatus)
            .HasColumnName("card_status")
            .HasMaxLength(20)
            .HasDefaultValue("Available")
            .IsRequired();

        // 7. Thời điểm tạo bản ghi (CreatedAt — kế thừa từ BaseEntity)
        // Mặc định lấy giờ hiện tại của PostgreSQL để tránh chênh lệch múi giờ
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // 8. Cột RowVersion (kế thừa từ BaseEntity) — kiểm soát xung đột đồng thời
        // Khi 2 người cùng sửa 1 thẻ, EF Core sẽ phát hiện và ném DbUpdateConcurrencyException
        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // 9. Bỏ qua các Computed Properties — không map vào cột DB
        // IsAvailable và IsLost là thuộc tính tính toán thuần C#, không lưu trong database
        builder.Ignore(c => c.IsAvailable);
        builder.Ignore(c => c.IsLost);
    }
}
