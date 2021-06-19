using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IFavoriteStoreService : IService<FavoriteStore, int>
    {

    }

    public class FavoriteStoreService : IFavoriteStoreService
    {
        private readonly IRepository<FavoriteStore, int> _iRepository;

        public FavoriteStoreService(IRepository<FavoriteStore, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<FavoriteStore> AddAsync(FavoriteStore entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(FavoriteStore entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<FavoriteStore> GetAll(params Expression<Func<FavoriteStore, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<FavoriteStore> GetByIdAsync(Expression<Func<FavoriteStore, bool>> predicate, params Expression<Func<FavoriteStore, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(FavoriteStore entity)
        {
            _iRepository.Update(entity);
        }
    }
}
