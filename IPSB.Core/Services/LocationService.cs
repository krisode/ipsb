using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface ILocationService : IService<Location, int>
    {
        Task AddRageAsync(List<Location> list);
    }

    public class LocationService : ILocationService
    {
        private readonly IRepository<Location, int> _iRepository;

        public LocationService(IRepository<Location, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<Location> AddAsync(Location entity)
        {
            return await _iRepository.AddAsync(entity);
        }
        public async Task AddRageAsync(List<Location> list)
        {
            await _iRepository.AddRangeAsync(list);
        }

        public void Delete(Location entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<Location> GetAll(params Expression<Func<Location, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<Location> GetByIdAsync(Expression<Func<Location, bool>> predicate, params Expression<Func<Location, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(Location entity)
        {
            _iRepository.Update(entity);
        }
    }
}
