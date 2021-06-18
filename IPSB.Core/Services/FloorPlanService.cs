using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IFloorPlanService : IService<FloorPlan, int>
    {

    }

    public class FloorPlanService : IFloorPlanService
    {
        private readonly IRepository<FloorPlan, int> _iRepository;

        public FloorPlanService(IRepository<FloorPlan, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<FloorPlan> AddAsync(FloorPlan entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(FloorPlan entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<FloorPlan> GetAll(params Expression<Func<FloorPlan, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<FloorPlan> GetByIdAsync(Expression<Func<FloorPlan, bool>> predicate, params Expression<Func<FloorPlan, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(FloorPlan entity)
        {
            _iRepository.Update(entity);
        }
    }
}
