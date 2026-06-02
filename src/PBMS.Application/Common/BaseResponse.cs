using System.Collections.Generic;

namespace PBMS.Application.Common;

/// <summary>
/// Chuẩn hóa cấu trúc kết quả trả về của API cho cả success và error cases.
/// Sử dụng cho mọi response không phân trang hoặc response trả về một đối tượng đơn.
/// </summary>
public class BaseResponse<T>
{
    /// <summary>
    /// Thông tin dữ liệu trả về.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Trạng thái thành công hay không.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Mã lỗi nếu có.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Thông báo kèm theo kết quả trả về.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Chi tiết lỗi validation nếu có.
    /// Key là tên property, Value là mảng các thông báo lỗi cho property đó.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Tạo response thành công với dữ liệu.
    /// </summary>
    /// <param name="data">Dữ liệu trả về.</param>
    /// <param name="message">Thông báo tùy chọn.</param>
    /// <returns>Response thành công.</returns>
    public static BaseResponse<T> Ok(T? data, string? message = null)
    {
        return new BaseResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// Tạo response lỗi.
    /// </summary>
    /// <param name="errorCode">Mã lỗi để xác định loại lỗi.</param>
    /// <param name="message">Thông báo lỗi.</param>
    /// <returns>Response lỗi.</returns>
    public static BaseResponse<T> Fail(string? errorCode = null, string? message = null)
    {
        return new BaseResponse<T>
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            Data = default
        };
    }

    /// <summary>
    /// Tạo response lỗi validation với chi tiết các lỗi từng field.
    /// </summary>
    /// <param name="errorCode">Mã lỗi, mặc định là VALIDATION_ERROR.</param>
    /// <param name="message">Thông báo lỗi, mặc định là mô tả lỗi validation.</param>
    /// <param name="errors">Dictionary chứa chi tiết lỗi của từng field.</param>
    /// <returns>Response lỗi validation.</returns>
    public static BaseResponse<T> ValidationFail(
        string? errorCode = null,
        string? message = null,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        return new BaseResponse<T>
        {
            Success = false,
            ErrorCode = errorCode ?? "VALIDATION_ERROR",
            Message = message ?? "One or more validation errors occurred.",
            Errors = errors,
            Data = default
        };
    }
}
