using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IShoppingListService : IService<ShoppingList, int>
    {

    }

    public class ShoppingListService : IShoppingListService
    {
        private readonly IRepository<ShoppingList, int> _iRepository;

        public ShoppingListService(IRepository<ShoppingList, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<ShoppingList> AddAsync(ShoppingList entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(ShoppingList entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<ShoppingList> GetAll(params Expression<Func<ShoppingList, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<ShoppingList> GetByIdAsync(Expression<Func<ShoppingList, bool>> predicate, params Expression<Func<ShoppingList, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(ShoppingList entity)
        {
            _iRepository.Update(entity);
        }
    }
}
