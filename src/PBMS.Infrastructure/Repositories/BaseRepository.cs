using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.Repositories
{
    /// <summary>
    /// Triển khai generic lớp BaseRepository làm nền tảng cho toàn bộ Repository sau này.
    /// Giúp loại bỏ việc viết lặp lại các lệnh thêm, xóa, sửa, tìm kiếm cơ bản bằng EF Core.
    /// </summary>
    /// <typeparam name="TEntity">Thực thể tương ứng trong C# (ví dụ: Account, Role, ...)</typeparam>
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        // AppDbContext quản lý kết nối và phiên làm việc với database
        protected readonly AppDbContext _context;
        // DbSet đại diện cho tập hợp (bảng) thực thể cụ thể trong database
        protected readonly DbSet<TEntity> _dbSet;

        // Constructor nhận vào AppDbContext thông qua cơ chế Dependency Injection
        public BaseRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>(); // Trỏ DbSet đến bảng tương ứng của TEntity
        }

        // 1. Tìm bản ghi theo ID khóa chính (Asynchronous)
        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        // 2. Lấy toàn bộ danh sách bản ghi
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // 3. Tìm kiếm danh sách bản ghi theo một biểu thức điều kiện (Predicate Lambda Expression)
        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        // 4. Tìm kiếm một bản ghi đầu tiên khớp với điều kiện truyền vào
        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // 5. Thêm mới một bản ghi vào DB
        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // 6. Thêm mới một danh sách các bản ghi cùng lúc
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // 7. Đánh dấu cập nhật một bản ghi (EF Core sẽ sinh câu lệnh UPDATE khi SaveChanges)
        public virtual void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        // 8. Đánh dấu cập nhật một danh sách bản ghi
        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        // 9. Xóa một bản ghi khỏi DB
        public virtual async Task RemoveAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        // 10. Xóa một danh sách bản ghi khỏi DB
        public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        // 11. Đếm số lượng bản ghi thỏa mãn điều kiện (nếu điều kiện null thì đếm tất cả)
        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            return predicate != null
                ? await _dbSet.CountAsync(predicate)
                : await _dbSet.CountAsync();
        }

        // 12. Kiểm tra xem có bản ghi nào khớp với điều kiện truyền vào hay không
        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // --- CÁC HÀM HELPER ĐỂ ĐẢM BẢO KHÔNG BỊ LỖI CHÊNH LỆCH KIỂU NULLABLE VỚI INTERFACE ---
        
        async Task<TEntity> IRepository<TEntity>.GetByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            return entity!;
        }

        async Task<TEntity> IRepository<TEntity>.FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entity = await FirstOrDefaultAsync(predicate);
            return entity!;
        }
    }
}
