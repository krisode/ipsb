using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IVisitStoreService : IService<VisitStore, int>
    {

    }

    public class VisitStoreService : IVisitStoreService
    {
        private readonly IRepository<VisitStore, int> _iRepository;

        public VisitStoreService(IRepository<VisitStore, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<VisitStore> AddAsync(VisitStore entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(VisitStore entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<VisitStore> GetAll(params Expression<Func<VisitStore, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<VisitStore> GetByIdAsync(Expression<Func<VisitStore, bool>> predicate, params Expression<Func<VisitStore, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(VisitStore entity)
        {
            _iRepository.Update(entity);
        }
    }
}
