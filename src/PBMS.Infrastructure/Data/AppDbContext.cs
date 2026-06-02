using Microsoft.EntityFrameworkCore;
using System.Reflection;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // =======================================================
    // KHU VỰC DÀNH CHO TEAM DEV:
    // Yêu cầu các thành viên thêm DbSet<T> của tính năng mình 
    // phụ trách vào bên dưới dòng này.
    // Ví dụ: public DbSet<Zone> Zones { get; set; }
    // =======================================================
    
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Role> Roles { get; set; }

    // =======================================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Lệnh này giúp EF Core tự động quét và áp dụng các file 
        // cấu hình bảng (Fluent API) mà team dev tạo ra
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}