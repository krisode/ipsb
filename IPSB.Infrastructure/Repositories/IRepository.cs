using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Infrastructure.Repositories
{
    public interface IRepository<T, TKey>
    {
        IQueryable<T> GetAll(params Expression<Func<T, object>>[] predicate);
        IQueryable<T> GetAllTwoConditionInclude(Expression<Func<T, bool>> firstCondition, Expression<Func<T, bool>> secondCondition, params Expression<Func<T, object>>[] includes);
        IQueryable<T> GetAllWhere(params Expression<Func<T, bool>>[] predicate);
        Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] expressions);
        bool IsExisted(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(List<T> list);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task<int> Save();
    }
}
