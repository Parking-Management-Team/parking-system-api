using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.Repositories
{
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        private readonly IServiceProvider _serviceProvider;

        public BaseRepository(AppDbContext context, IServiceProvider serviceProvider = null!)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
            _serviceProvider = serviceProvider;
        }

        private int? GetCurrentUserId()
        {
            if (_serviceProvider != null)
            {
                var currentUserService = _serviceProvider.GetService<ICurrentUserService>();
                return currentUserService?.UserId;
            }
            return null;
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

        // 9. Xóa một bản ghi khỏi DB (Hoặc Xóa mềm)
        public virtual async Task RemoveAsync(TEntity entity)
        {
            if (entity is PBMS.Domain.Entities.ISoftDeletable softDeletable)
            {
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = DateTime.UtcNow;
                softDeletable.DeletedBy = GetCurrentUserId();
                _context.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                _dbSet.Remove(entity);
            }
            await Task.CompletedTask;
        }

        // 10. Xóa một danh sách bản ghi khỏi DB (Hoặc Xóa mềm)
        public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity is PBMS.Domain.Entities.ISoftDeletable softDeletable)
                {
                    softDeletable.IsDeleted = true;
                    softDeletable.DeletedAt = DateTime.UtcNow;
                    softDeletable.DeletedBy = GetCurrentUserId();
                    _context.Entry(entity).State = EntityState.Modified;
                }
                else
                {
                    _dbSet.Remove(entity);
                }
            }
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
        // 13. Lưu các thay đổi vào cơ sở dữ liệu (Triển khai thực tế thông qua DbContext)
        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
