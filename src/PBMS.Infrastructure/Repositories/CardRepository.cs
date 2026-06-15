using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

public class CardRepository : BaseRepository<Card>, ICardRepository
{
    public CardRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Card?> GetByCardCodeAsync(string cardCode)
    {
        var normalized = cardCode.Trim().ToUpperInvariant();

        return await _dbSet
            .FirstOrDefaultAsync(c => c.CardCode == normalized);
    }

    public async Task<bool> IsCardCodeExistsAsync(string cardCode)
    {
        var normalized = cardCode.Trim().ToUpperInvariant();
        return await _dbSet.AnyAsync(c => c.CardCode == normalized);
    }

    public async Task<bool> IsCardInActiveSessionAsync(int cardId)
    {
        return await _context.Set<ParkingSession>()
            .AnyAsync(s => s.CardId == cardId && s.SessionStatus.ToUpper() == "ACTIVE");
    }
}
