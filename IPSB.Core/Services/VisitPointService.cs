using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IVisitPointService : IService<VisitPoint, int>
    {

    }

    public class VisitPointService : IVisitPointService
    {
        private readonly IRepository<VisitPoint, int> _iRepository;

        public VisitPointService(IRepository<VisitPoint, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<VisitPoint> AddAsync(VisitPoint entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(VisitPoint entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<VisitPoint> GetAll(params Expression<Func<VisitPoint, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<VisitPoint> GetByIdAsync(Expression<Func<VisitPoint, bool>> predicate, params Expression<Func<VisitPoint, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(VisitPoint entity)
        {
            _iRepository.Update(entity);
        }
    }
}
