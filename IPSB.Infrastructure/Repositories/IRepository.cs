using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Infrastructure.Repositories
{
    public interface IRepository<T, TKey>
    {
        IQueryable<T> GetAll(params Expression<Func<T, object>>[] predicate);
        Task<T> GetByIdAsync(TKey id);
        Task<T> AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        void DeleteRange(Expression<Func<T, bool>> predicate);
    }
}
