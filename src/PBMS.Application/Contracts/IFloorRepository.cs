using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

/// <summary>
/// Interface repository cho entity Floor.
/// Cung cấp các phép truy xuất dữ liệu chuyên biệt cho quản lý tầng.
/// </summary>
public interface IFloorRepository : IRepository<Floor>
{
    /// <summary>
    /// Lấy tất cả tầng theo ID tòa nhà bất đồng bộ.
    /// </summary>
    /// <param name="buildingId">ID tòa nhà.</param>
    /// <returns>Tập hợp các tầng thuộc tòa nhà.</returns>
    Task<IEnumerable<Floor>> GetFloorsByBuildingIdAsync(int buildingId);

    /// <summary>
    /// Kiểm tra số tầng đã tồn tại trong một tòa nhà cụ thể hay chưa.
    /// </summary>
    /// <param name="floorNumber">Số tầng cần kiểm tra.</param>
    /// <param name="buildingId">ID tòa nhà.</param>
    /// <returns>True nếu đã tồn tại, ngược lại false.</returns>
    Task<bool> FloorNumberExistsInBuildingAsync(int floorNumber, int buildingId);

    /// <summary>
    /// Lấy thông tin tầng kèm theo các dữ liệu liên quan (Building, Zones) bất đồng bộ.
    /// </summary>
    /// <param name="id">ID của tầng.</param>
    /// <returns>Thông tin tầng chi tiết hoặc null nếu không tìm thấy.</returns>
    Task<Floor?> GetFloorWithDetailsAsync(int id);
}
