namespace PBMS.Domain.Entities;

public class Role : BaseEntity
{
    public string RoleName { get; set; } = null!;
    public string? Description { get; set; }

    // Từ khóa 'virtual' cho phép Entity Framework Core sử dụng cơ chế Lazy Loading (Tải chậm).
    // Khi gọi 'role.Accounts', EF Core sẽ tự động truy vấn DB để lấy danh sách Account liên quan nếu chưa được load sẵn.
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}