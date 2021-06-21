using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IProductGroupService : IService<ProductGroup, int>
    {

    }

    public class ProductGroupService : IProductGroupService
    {
        private readonly IRepository<ProductGroup, int> _iRepository;

        public ProductGroupService(IRepository<ProductGroup, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<ProductGroup> AddAsync(ProductGroup entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(ProductGroup entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<ProductGroup> GetAll(params Expression<Func<ProductGroup, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<ProductGroup> GetByIdAsync(Expression<Func<ProductGroup, bool>> predicate, params Expression<Func<ProductGroup, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(ProductGroup entity)
        {
            _iRepository.Update(entity);
        }
    }
}
