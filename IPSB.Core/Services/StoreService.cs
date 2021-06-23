using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IStoreService : IService<Store, int>
    {

    }

    public class StoreService : IStoreService
    {
        private readonly IRepository<Store, int> _iRepository;

        public StoreService(IRepository<Store, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<Store> AddAsync(Store entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(Store entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<Store> GetAll(params Expression<Func<Store, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<Store> GetByIdAsync(Expression<Func<Store, bool>> predicate, params Expression<Func<Store, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(Store entity)
        {
            _iRepository.Update(entity);
        }
    }
}
