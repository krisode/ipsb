using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IProductService : IService<Product, int>
    {

    }

    public class ProductService : IProductService
    {
        private readonly IRepository<Product, int> _iRepository;

        public ProductService(IRepository<Product, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<Product> AddAsync(Product entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(Product entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<Product> GetAll(params Expression<Func<Product, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<Product> GetByIdAsync(Expression<Func<Product, bool>> predicate, params Expression<Func<Product, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(Product entity)
        {
            _iRepository.Update(entity);
        }
    }
}
