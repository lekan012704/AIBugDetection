using SharedKernel;
using System.Linq.Expressions;

namespace Application.Abstractions.GenericRepository
{
    public interface IRepositoryAsync<TEntity, TEntityId>
    where TEntity : Entity<TEntityId>
    {
        Task<TEntity?> GetByIdAsync(TEntityId id, CancellationToken cancellationToken = default);
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate);
        IQueryable<TEntity> GetPagination(int pageNumber, int pageSize);
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}
