using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface ILocationTypeService : IService<LocationType, int>
    {

    }

    public class LocationTypeService : ILocationTypeService
    {
        private readonly IRepository<LocationType, int> _iRepository;

        public LocationTypeService(IRepository<LocationType, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<LocationType> AddAsync(LocationType entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(LocationType entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<LocationType> GetAll(params Expression<Func<LocationType, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<LocationType> GetByIdAsync(Expression<Func<LocationType, bool>> predicate, params Expression<Func<LocationType, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(LocationType entity)
        {
            _iRepository.Update(entity);
        }
    }
}
