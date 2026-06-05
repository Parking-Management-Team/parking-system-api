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
        /// <returns>BaseResponse chứa zone DTO vừa tạo hoặc thông tin lỗi.</returns>
        Task<BaseResponse<ZoneDto>> CreateZoneAsync(ZoneCreateRequest request);

        /// <summary>
        /// Lấy zone theo ID bất đồng bộ.
        /// </summary>
        /// <param name="id">ID zone.</param>
        /// <returns>BaseResponse chứa zone DTO hoặc thông tin lỗi.</returns>
        Task<BaseResponse<ZoneDto>> GetZoneByIdAsync(int id);

        /// <summary>
        /// Lấy tất cả zone bất đồng bộ.
        /// </summary>
        /// <returns>BaseResponse chứa danh sách zone DTO hoặc thông tin lỗi.</returns>
        Task<BaseResponse<IEnumerable<ZoneDto>>> GetAllZonesAsync();

        /// <summary>
        /// Lấy tất cả zone cho một floor cụ thể bất đồng bộ.
        /// </summary>
        /// <param name="floorId">ID floor.</param>
        /// <returns>BaseResponse chứa danh sách zone DTO cho floor hoặc thông tin lỗi.</returns>
        Task<BaseResponse<IEnumerable<ZoneDto>>> GetZonesByFloorAsync(int floorId);

        /// <summary>
        /// Lấy zone theo phân trang bất đồng bộ.
        /// </summary>
        /// <param name="pageIndex">Chỉ số trang (bắt đầu từ 1).</param>
        /// <param name="pageSize">Số mục trên mỗi trang.</param>
        /// <returns>BaseResponse chứa kết quả phân trang zone DTO hoặc thông tin lỗi.</returns>
        Task<BaseResponse<PagedResult<ZoneDto>>> GetZonesPagedAsync(int pageIndex, int pageSize);

        /// <summary>
        /// Cập nhật zone hiện có bất đồng bộ.
        /// </summary>
        /// <param name="id">ID zone.</param>
        /// <param name="request">Yêu cầu cập nhật zone.</param>
        /// <returns>BaseResponse chứa zone DTO đã cập nhật hoặc thông tin lỗi.</returns>
        Task<BaseResponse<ZoneDto>> UpdateZoneAsync(int id, ZoneUpdateRequest request);

        /// <summary>
        /// Xóa zone bất đồng bộ.
        /// </summary>
        /// <param name="id">ID zone.</param>
        /// <returns>BaseResponse trả về thành công hoặc lỗi.</returns>
        Task<BaseResponse<string>> DeleteZoneAsync(int id);
    }
}
