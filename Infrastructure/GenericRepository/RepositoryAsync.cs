using Application.Abstractions.Data;
using Application.Abstractions.GenericRepository;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System.Linq.Expressions;


namespace Infrastructure.GenericRepository
{
    public class RepositoryAsync<TEntity, TEntityId> : Application.Abstractions.GenericRepository.IRepositoryAsync<TEntity, TEntityId>
    where TEntity : Entity<TEntityId>
    {
        protected readonly IUnitOfWork dbContext;
        internal DbSet<TEntity> EntitySet;

        protected RepositoryAsync(IUnitOfWork dbContext)
        {
            this.dbContext = dbContext;
            EntitySet = dbContext.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(TEntityId id, CancellationToken cancellationToken = default)
        {
            return await EntitySet
                .FirstOrDefaultAsync(x => x.Id!.Equals(id), cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return EntitySet
                .AsNoTracking();
        }

        public virtual IQueryable<TEntity> GetPagination(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            return EntitySet
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await EntitySet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity;
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            EntitySet.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            EntitySet.Remove(entity);
            await Task.CompletedTask;
        }
        public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            if (!entities.Any()) throw new ArgumentNullException(nameof(entities));

            EntitySet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public virtual IQueryable<TEntity> GetQueryable()
        {
            return EntitySet.AsQueryable();
        }
        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
        {
            return EntitySet.Where(predicate).AsQueryable<TEntity>();
        }
    }
}
