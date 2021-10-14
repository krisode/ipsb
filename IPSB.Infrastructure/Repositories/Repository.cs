using IPSB.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Infrastructure.Repositories
{
    public class Repository<T, TKey> : IRepository<T, TKey> where T : class
    {
        protected IndoorPositioningContext _dbContext;

        public Repository(IndoorPositioningContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<T> GetAll(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> queryList = _dbContext.Set<T>().AsNoTracking();

            foreach (var expression in includes)
            {
                queryList = queryList.Include(expression);
            }
            return queryList;
        }
        public IQueryable<T> GetAllTwoConditionInclude(Expression<Func<T, bool>> firstCondition, Expression<Func<T, bool>> secondCondition, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> queryList = _dbContext.Set<T>().AsNoTracking();
            queryList = queryList.Where(firstCondition);
            queryList = queryList.Where(secondCondition);

            if (includes.Length > 0)
            {
                foreach (var expression in includes)
                {
                    queryList = queryList.Include(expression);
                }
            }
            
            return queryList;
        }

        public IQueryable<T> GetAllWhere(params Expression<Func<T, bool>>[] includes)
        {
            IQueryable<T> queryList = _dbContext.Set<T>().AsNoTracking();

            foreach (var expression in includes)
            {
                queryList = queryList.Where(expression);
            }
            return queryList;
        }

        public async Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> queryList = _dbContext.Set<T>().AsNoTracking();
            
            if (includes.Length > 0)
            {
                foreach (var expression in includes)
                {
                    queryList = queryList.Include(expression);
                }
            }
            return await queryList.FirstOrDefaultAsync(predicate);

        }

        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException($"{nameof(AddAsync)} entity must not be null");
            }
            await _dbContext.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(List<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException($"{nameof(AddRangeAsync)} list entity must not be null");
            }
            await _dbContext.AddRangeAsync(list);
        }

        public void Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException($"{nameof(AddAsync)} entity must not be null");
            }
            _dbContext.Update(entity);

        }

        public void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException($"{nameof(AddAsync)} entity must not be null");
            }
            _dbContext.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbContext.RemoveRange(entities);
        }

        public async Task<int> Save()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbContext.UpdateRange(entities);
        }
    }
}
