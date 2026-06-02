using PBMS.Application.Common.Exceptions;

namespace PBMS.Application.Common.Exceptions
{
    /// <summary>
    /// Ngoại lệ xung đột đồng thời (Concurrency Conflict).
    /// Được ném khi phát hiện dữ liệu đã bị thay đổi bởi người dùng khác trong quá trình sửa/xóa.
    /// Thường bắt nguồn từ DbUpdateConcurrencyException của EF Core.
    /// </summary>
    public class ConcurrencyException : AppException
    {
        /// <summary>
        /// Khởi tạo ConcurrencyException với thông báo lỗi mặc định.
        /// </summary>
        public ConcurrencyException()
            : base("CONFLICT", "The data has been modified by another user. Please refresh and try again.")
        {
        }

        /// <summary>
        /// Khởi tạo ConcurrencyException với thông báo lỗi tùy chỉnh.
        /// </summary>
        /// <param name="message">Thông báo lỗi cụ thể.</param>
        public ConcurrencyException(string message)
            : base("CONFLICT", message)
        {
        }

        /// <summary>
        /// Khởi tạo ConcurrencyException với thông báo lỗi và exception gốc (inner exception).
        /// Dùng khi bắt DbUpdateConcurrencyException từ EF Core và muốn giữ lại exception gốc.
        /// </summary>
        /// <param name="message">Thông báo lỗi.</param>
        /// <param name="innerException">Exception gốc từ EF Core.</param>
        public ConcurrencyException(string message, Exception innerException)
            : base("CONFLICT", message, innerException)
        {
        }
    }
}
