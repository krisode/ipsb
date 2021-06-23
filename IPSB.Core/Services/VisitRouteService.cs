using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IVisitRouteService : IService<VisitRoute, int>
    {

    }

    public class VisitRouteService : IVisitRouteService
    {
        private readonly IRepository<VisitRoute, int> _iRepository;

        public VisitRouteService(IRepository<VisitRoute, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<VisitRoute> AddAsync(VisitRoute entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(VisitRoute entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<VisitRoute> GetAll(params Expression<Func<VisitRoute, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<VisitRoute> GetByIdAsync(Expression<Func<VisitRoute, bool>> predicate, params Expression<Func<VisitRoute, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(VisitRoute entity)
        {
            _iRepository.Update(entity);
        }
    }
}
