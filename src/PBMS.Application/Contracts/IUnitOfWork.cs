using System.Threading.Tasks;

namespace PBMS.Application.Contracts;

/// <summary>
/// Giao diện Unit of Work để quản lý transaction và lưu thay đổi đồng thời trên nhiều repository.
/// Tham chiếu SRS: §8.1
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Bắt đầu một transaction mới.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commit transaction hiện tại.
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Rollback transaction hiện tại.
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// Lưu tất cả các thay đổi vào database.
    /// </summary>
    Task<int> SaveChangesAsync();
}
