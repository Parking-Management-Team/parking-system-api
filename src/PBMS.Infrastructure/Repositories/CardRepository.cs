using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Triển khai ICardRepository — thực thi các truy vấn dữ liệu cụ thể liên quan đến Card.
/// Kế thừa toàn bộ khả năng CRUD từ BaseRepository&lt;Card&gt;.
///
/// Các truy vấn đặc biệt được thêm vào đây (ngoài CRUD chung):
///   - GetByCardCodeAsync  : tìm theo mã thẻ (Scenario 3)
///   - IsCardCodeExistsAsync : kiểm tra trùng mã khi tạo mới (Scenario 1)
///   - IsCardInActiveSessionAsync : kiểm tra thẻ đang bận (Scenario 2)
/// </summary>
public class CardRepository : BaseRepository<Card>, ICardRepository
{
    /// <summary>
    /// Constructor truyền AppDbContext xuống BaseRepository qua Dependency Injection.
    /// </summary>
    public CardRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// [Scenario 3] Tìm thẻ theo mã định danh CardCode.
    ///
    /// Truy vấn: SELECT * FROM card WHERE UPPER(card_code) = UPPER(@cardCode)
    /// Dùng ToUpper() để tìm kiếm không phân biệt chữ hoa/thường.
    /// </summary>
    public async Task<Card?> GetByCardCodeAsync(string cardCode)
    {
        // Trim và Upper để đồng nhất với cách lưu trữ trong DB
        var normalized = cardCode.Trim().ToUpper();

        return await _dbSet
            .FirstOrDefaultAsync(c => c.CardCode == normalized);
    }

    /// <summary>
    /// [Scenario 1] Kiểm tra mã thẻ đã tồn tại trong hệ thống chưa.
    ///
    /// Dùng AnyAsync thay vì FirstOrDefaultAsync vì chỉ cần biết có/không,
    /// không cần load toàn bộ entity → hiệu quả hơn.
    ///
    /// Truy vấn: SELECT EXISTS (SELECT 1 FROM card WHERE card_code = @cardCode)
    /// </summary>
    public async Task<bool> IsCardCodeExistsAsync(string cardCode)
    {
        var normalized = cardCode.Trim().ToUpper();

        // AnyAsync chỉ kiểm tra sự tồn tại → DB chỉ cần tìm 1 bản ghi phù hợp
        // Nhanh hơn nhiều so với load toàn bộ entity
        return await _dbSet.AnyAsync(c => c.CardCode == normalized);
    }

    /// <summary>
    /// [Scenario 2] Kiểm tra thẻ có đang được dùng trong ParkingSession ACTIVE không.
    ///
    /// Truy vấn:
    ///   SELECT EXISTS (
    ///     SELECT 1 FROM parking_session
    ///     WHERE card_id = @cardId AND session_status = 'Active'
    ///   )
    ///
    /// Lưu ý: ParkingSession chưa có đầy đủ properties, nhưng vì EF Core
    /// dùng Navigation Property nên ta có thể query qua DbContext trực tiếp.
    /// Khi ParkingSession được implement đầy đủ, query này sẽ hoạt động đúng.
    /// </summary>
    public async Task<bool> IsCardInActiveSessionAsync(int cardId)
    {
        // Truy vấn qua DbContext để tránh circular dependency
        // _context.Set<ParkingSession>() — đọc bảng parking_session trực tiếp
        return await _context.Set<ParkingSession>()
            .AnyAsync(s =>
                s.CardId == cardId &&           // Thẻ này đang được gán cho session nào đó
                s.SessionStatus == "Active"      // Session đó vẫn đang mở (xe chưa ra)
            );
    }
}
