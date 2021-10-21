using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IBuildingService : IService<Building, int>
    {
        bool IsExisted(Expression<Func<Building, bool>> predicate);
    }

    public class BuildingService : IBuildingService
    {
        private readonly IRepository<Building, int> _iRepository;

        public BuildingService(IRepository<Building, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<Building> AddAsync(Building entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(Building entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<Building> GetAll(params Expression<Func<Building, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<Building> GetByIdAsync(Expression<Func<Building, bool>> predicate, params Expression<Func<Building, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public bool IsExisted(Expression<Func<Building, bool>> predicate)
        {
            return _iRepository.IsExisted(predicate);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(Building entity)
        {
            _iRepository.Update(entity);
        }
    }
}
