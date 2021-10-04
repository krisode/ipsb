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
    public interface IShoppingItemService : IService<ShoppingItem, int>
    {
        Task AddRangeAsync(List<ShoppingItem> list);
        void DeleteRange(ICollection<ShoppingItem> list);
    }

    public class ShoppingItemService : IShoppingItemService
    {
        private readonly IRepository<ShoppingItem, int> _iRepository;

        public ShoppingItemService(IRepository<ShoppingItem, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<ShoppingItem> AddAsync(ShoppingItem entity)
        {
            return await _iRepository.AddAsync(entity);
        }
        public async Task AddRangeAsync(List<ShoppingItem> list)
        {
            await _iRepository.AddRangeAsync(list);
        }

        public void Delete(ShoppingItem entity)
        {
            _iRepository.Delete(entity);
        }

        public void DeleteRange(ICollection<ShoppingItem> list)
        {
            _iRepository.DeleteRange(list);
        }

        public IQueryable<ShoppingItem> GetAll(params Expression<Func<ShoppingItem, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<ShoppingItem> GetByIdAsync(Expression<Func<ShoppingItem, bool>> predicate, params Expression<Func<ShoppingItem, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(ShoppingItem entity)
        {
            _iRepository.Update(entity);
        }
    }
}
