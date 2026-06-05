using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

/// <summary>
/// Interface repository cho entity Zone.
/// Cung cấp các phép truy xuất dữ liệu chuyên biệt cho quản lý Zone.
/// </summary>
public interface IZoneRepository : IRepository<Zone>
{
    /// <summary>
    /// Lấy tất cả zone theo floor bất đồng bộ.
    /// </summary>
    /// <param name="floorId">ID của floor.</param>
    /// <returns>Tập hợp zone trong floor.</returns>
    Task<IEnumerable<Zone>> GetZonesByFloorIdAsync(int floorId);

    /// <summary>
    /// Lấy tất cả zone cho một loại xe cụ thể bất đồng bộ.
    /// </summary>
    /// <param name="vehicleTypeId">ID loại xe.</param>
    /// <returns>Tập hợp zone cho loại xe đó.</returns>
    Task<IEnumerable<Zone>> GetZonesByVehicleTypeIdAsync(int vehicleTypeId);

    /// <summary>
    /// Kiểm tra tên zone đã tồn tại trong floor cụ thể hay chưa bất đồng bộ.
    /// </summary>
    /// <param name="name">Tên zone.</param>
    /// <param name="floorId">ID floor.</param>
    /// <returns>True nếu tên đã tồn tại trong floor, ngược lại false.</returns>
    Task<bool> ZoneNameExistsInFloorAsync(string name, int floorId);

    /// <summary>
    /// Lấy zone kèm dữ liệu liên quan floor và parking slots bất đồng bộ.
    /// </summary>
    /// <param name="id">ID zone.</param>
    /// <returns>Zone với dữ liệu liên quan; nếu không có trả về null.</returns>
    Task<Zone?> GetZoneWithDetailsAsync(int id);
}
