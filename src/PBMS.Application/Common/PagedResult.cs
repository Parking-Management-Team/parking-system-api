namespace PBMS.Application.Common;

/// <summary>
/// PagedResult dùng để chuẩn hóa dữ liệu trả về cho các danh sách cần phân trang.
/// Chứa dữ liệu, thông tin paging và trạng thái kết quả.
/// </summary>
public class PagedResult<T>
{
    /// <summary>
    /// Danh sách dữ liệu của trang hiện tại.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Tổng số bản ghi của toàn bộ danh sách.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Số trang tổng cộng.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Trang hiện tại được trả về.
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// Kích thước trang.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Trạng thái thành công hay không.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Thông báo bổ sung nếu cần.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Tạo PagedResult chuẩn từ dữ liệu và thông tin phân trang.
    /// </summary>
    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize, string? message = null)
    {
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalPages = totalPages,
            Message = message,
            Success = true
        };
    }
}
