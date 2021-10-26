using ApplicationCore.Services;
using IPSB;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface ILocationService : IService<Location, int>
    {
        static int TYPE_POINT_ON_ROUTE = 2;
        Task AddRangeAsync(List<Location> list);
        void DeleteRange(List<int> ids);
        Task DeleteById(int id);
        void Disable(List<int> ids);
        Task<int?> CreateLocationJson(string json);
        Task<bool> UpdateLocationJson(int? id, string json);
        Task<bool> DisableLocation(int? id);
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

        public async Task DeleteById(int id)
        {
            var entity = await _iRepository.GetByIdAsync(_ => _.Id == id);
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

        public async Task<bool> DisableLocation(int? id)
        {
            if (id != null)
            {
                var entity = await _iRepository.GetByIdAsync(_ => _.Id == id);
                if (entity != null)
                {
                    var inActiveStatus = "Inactive";
                    entity.Status = inActiveStatus;
                    _iRepository.Update(entity);
                    return true;
                }
            }
            return false;
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

        public async Task<int?> CreateLocationJson(string json)
        {
            var activeStatus = "Active";
            int? locationId = null;
            if (!string.IsNullOrEmpty(json))
            {

                var locationEntity = JsonConvert.DeserializeObject<Location>(json);
                if (locationEntity != null && locationEntity.Id == 0)
                {
                    locationEntity.Status = activeStatus;
                    var locationToCreate = await AddAsync(locationEntity);
                    await Save();
                    locationId = locationToCreate.Id;
                }
            }
            return locationId;
        }

        public void Update(Location entity)
        {
            _iRepository.Update(entity);
        }


        public async Task<bool> UpdateLocationJson(int? locationId, string json)
        {
            if (!string.IsNullOrEmpty(json) && locationId != null)
            {
                var locationEntity = JsonConvert.DeserializeObject<Location>(json);
                if (locationEntity != null && locationEntity.Id == 0)
                {
                    var updateEntity = await GetByIdAsync(_ => _.Id == locationId);
                    if (updateEntity != null)
                    {
                        updateEntity.X = locationEntity.X;
                        updateEntity.Y = locationEntity.Y;
                        return true;
                    }

                }
            }
            return false;
        }
    }
}
