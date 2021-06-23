using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface ILocatorTagService : IService<LocatorTag, int>
    {
        IQueryable<LocatorTag> GetAllWhere(params Expression<Func<LocatorTag, bool>>[] includes);
    }

    public class LocatorTagService : ILocatorTagService
    {
        private readonly IRepository<LocatorTag, int> _iRepository;

        public LocatorTagService(IRepository<LocatorTag, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<LocatorTag> AddAsync(LocatorTag entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(LocatorTag entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<LocatorTag> GetAll(params Expression<Func<LocatorTag, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }
        public IQueryable<LocatorTag> GetAllWhere(params Expression<Func<LocatorTag, bool>>[] includes)
        {
            return _iRepository.GetAllWhere(includes);
        }

        public async Task<LocatorTag> GetByIdAsync(Expression<Func<LocatorTag, bool>> predicate, params Expression<Func<LocatorTag, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(LocatorTag entity)
        {
            _iRepository.Update(entity);
        }
    }
}
