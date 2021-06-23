using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    public interface IService<T, TKey>
    {
        IQueryable<T> GetAll(params Expression<Func<T, object>>[] includes);
        Task<T> AddAsync(T entity);
        Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        void Update(T entity);
        void Delete(T entity);
        Task<int> Save();
    }
}
