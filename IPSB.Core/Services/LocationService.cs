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
        static int TYPE_POINT_ON_ROUTE = 2;
        Task AddRangeAsync(List<Location> list);
        void DeleteRange(List<int> ids);
        void Disable(List<int> ids);
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
        public async Task AddRangeAsync(List<Location> list)
        {
            await _iRepository.AddRangeAsync(list);
        }

        public void Delete(Location entity)
        {
            _iRepository.Delete(entity);
        }
        public void DeleteRange(List<int> ids)
        {
            var lstRemove = _iRepository.GetAll()
           .Where(_ => ids.Contains(_.Id) && _.LocationTypeId == ILocationService.TYPE_POINT_ON_ROUTE);
            _iRepository.DeleteRange(lstRemove);
        }

        public void Disable(List<int> ids)
        {
            var lstLocation = _iRepository.GetAll()
            .Where(_ => ids.Contains(_.Id) && _.LocationTypeId != ILocationService.TYPE_POINT_ON_ROUTE)
            .ToList();
            lstLocation.ForEach(loc => loc.Status = "Inactive");
            _iRepository.UpdateRange(lstLocation);

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
