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
    public interface IEdgeService : IService<Edge, int>
    {
        Task AddRageAsync(List<Edge> list);
        void DeleteRange(List<int> list);
    }

    public class EdgeService : IEdgeService
    {
        private readonly IRepository<Edge, int> _iRepository;

        public EdgeService(IRepository<Edge, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<Edge> AddAsync(Edge entity)
        {
            return await _iRepository.AddAsync(entity);
        }
        public async Task AddRageAsync(List<Edge> list)
        {
            await _iRepository.AddRangeAsync(list);
        }

        public void Delete(Edge entity)
        {
            _iRepository.Delete(entity);
        }

        public void DeleteRange(List<int> list)
        {
            _iRepository.DeleteRange(_ => list.Contains(_.Id));
        }

        public IQueryable<Edge> GetAll(params Expression<Func<Edge, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<Edge> GetByIdAsync(Expression<Func<Edge, bool>> predicate, params Expression<Func<Edge, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(Edge entity)
        {
            _iRepository.Update(entity);
        }
    }
}
