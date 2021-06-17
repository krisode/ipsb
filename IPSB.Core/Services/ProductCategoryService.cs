using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IProductCategoryService : IService<ProductCategory, int>
    {

    }

    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IRepository<ProductCategory, int> _iRepository;

        public ProductCategoryService(IRepository<ProductCategory, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<ProductCategory> AddAsync(ProductCategory entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(ProductCategory entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<ProductCategory> GetAll(params Expression<Func<ProductCategory, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<ProductCategory> GetByIdAsync(Expression<Func<ProductCategory, bool>> predicate, params Expression<Func<ProductCategory, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(ProductCategory entity)
        {
            _iRepository.Update(entity);
        }
    }
}
