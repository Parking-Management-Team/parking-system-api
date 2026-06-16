using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

/// <summary>
/// Interface repository cho entity Building.
/// </summary>
public interface IBuildingRepository : IRepository<Building>
{
    /// <summary>
    /// Kiểm tra xem mã tòa nhà đã tồn tại chưa.
    /// </summary>
    /// <summary>
    /// Lấy chi tiết tòa nhà kèm theo danh sách các tầng.
    /// </summary>
    Task<Building?> GetBuildingWithDetailsAsync(int id);
}
