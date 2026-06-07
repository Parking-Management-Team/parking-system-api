using PBMS.Application.Common;
using PBMS.Application.ParkingStructure.DTOs;

namespace PBMS.Application.ParkingStructure.Interfaces
{
    /// <summary>
    /// Interface service cho nghiệp vụ Zone.
    /// Định nghĩa các thao tác tạo, cập nhật, lấy và xóa zone.
    /// </summary>
    public interface IZoneService
    {
        /// <summary>
        /// Tạo zone mới bất đồng bộ.
        /// </summary>
        /// <param name="request">Yêu cầu tạo zone.</param>
        /// <returns>Zone DTO vừa tạo.</returns>
        Task<ZoneDto> CreateZoneAsync(ZoneCreateRequest request);

        /// <summary>
        /// Lấy zone theo ID bất đồng bộ.
        /// </summary>
        /// <param name="id">ID zone.</param>
        /// <returns>Zone DTO.</returns>
        Task<ZoneDto> GetZoneByIdAsync(int id);

        /// <summary>
        /// Lấy tất cả zone bất đồng bộ.
        /// </summary>
        /// <returns>Danh sách zone DTO.</returns>
        Task<IEnumerable<ZoneDto>> GetAllZonesAsync();

        /// <summary>
        /// Lấy tất cả zone cho một floor cụ thể bất đồng bộ.
        /// </summary>
        /// <param name="floorId">ID floor.</param>
        /// <returns>Danh sách zone DTO cho floor.</returns>
        Task<IEnumerable<ZoneDto>> GetZonesByFloorAsync(int floorId);

        /// <summary>
        /// Lấy zone theo phân trang bất đồng bộ.
        /// </summary>
        /// <param name="pageIndex">Chỉ số trang (bắt đầu từ 1).</param>
        /// <param name="pageSize">Số mục trên mỗi trang.</param>
        /// <returns>Kết quả phân trang zone DTO.</returns>
        Task<PagedResult<ZoneDto>> GetZonesPagedAsync(int pageIndex, int pageSize);

        /// <summary>
        /// Cập nhật zone hiện có bất đồng bộ.
        /// </summary>
        /// <param name="id">ID zone.</param>
        /// <param name="request">Yêu cầu cập nhật zone.</param>
        /// <returns>Zone DTO đã cập nhật.</returns>
        Task<ZoneDto> UpdateZoneAsync(int id, ZoneUpdateRequest request);

        /// <summary>
        /// Xóa zone bất đồng bộ.
        /// </summary>
        /// <param name="id">ID zone.</param>
        /// <returns>Bình thường trả về void hoặc task, ở đây có thể trả về bool hoặc void.</returns>
        Task DeleteZoneAsync(int id);
    }
}
