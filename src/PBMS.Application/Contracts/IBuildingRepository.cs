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
    Task<bool> BuildingCodeExistsAsync(string code);

    /// <summary>
    /// Lấy chi tiết tòa nhà kèm theo danh sách các tầng.
    /// </summary>
    Task<Building?> GetBuildingWithDetailsAsync(int id);

    /// <summary>
    /// Lấy tổng sức chứa của xe máy trong tòa nhà (bằng tổng sức chứa các Zone xe máy).
    /// </summary>
    Task<int> GetTotalMotorcycleCapacityAsync(int buildingId);

    /// <summary>
    /// Lấy tổng General Capacity của Building cho một loại xe cụ thể.
    /// Tính bằng tổng capacity của tất cả Zone có AccessType = General
    /// và VehicleTypeId khớp, thuộc các Floor trong Building đó.
    /// Dùng để kiểm tra capacity khi tạo Booking mới.
    /// </summary>
    Task<int> GetTotalGeneralCapacityAsync(int buildingId, int vehicleTypeId);
}

