using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PBMS.Application.Contracts
{
    /// <summary>
    /// Generic repository interface that defines common CRUD operations and query functionality.
    /// This interface serves as a contract for all specific repository implementations in the Infrastructure layer.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity managed by the repository.</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Retrieves an entity by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        Task<TEntity?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all entities of the given type asynchronously.
        /// </summary>
        /// <returns>A collection of all entities.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Retrieves entities that match the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The condition to filter entities.</param>
        /// <returns>A collection of entities that match the condition.</returns>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Retrieves a single entity that matches the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The condition to filter the entity.</param>
        /// <returns>The first entity that matches the condition; otherwise, null.</returns>
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Adds a new entity to the repository asynchronously.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Adds a range of entities to the repository asynchronously.
        /// </summary>
        /// <param name="entities">The collection of entities to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Updates an existing entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Updates a range of entities in the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to update.</param>
        void UpdateRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Removes an entity from the repository asynchronously.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveAsync(TEntity entity);

        /// <summary>
        /// Removes a range of entities from the repository asynchronously.
        /// </summary>
        /// <param name="entities">The collection of entities to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Counts the number of entities that match the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The condition to filter entities. If null, counts all entities.</param>
        /// <returns>The count of entities matching the condition.</returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

        // Xác định xem có thực thể nào thỏa mãn điều kiện truyền vào hay không.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Lưu tất cả các thay đổi đã thực hiện trong ngữ cảnh này vào cơ sở dữ liệu một cách bất đồng bộ.
        /// </summary>
        /// <returns> Một task đại diện cho operation lưu trữ bất đồng bộ. Kết quả task chứa số lượng các mục trạng thái đã được ghi vào cơ sở dữ liệu. </returns>
        Task<int> SaveChangesAsync();
    }
}
