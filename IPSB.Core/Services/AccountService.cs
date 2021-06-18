using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface IAccountService : IService<Account, int>
    {

    }

    public class AccountService : IAccountService
    {
        private readonly IRepository<Account, int> _iRepository;

        public AccountService(IRepository<Account, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<Account> AddAsync(Account entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(Account entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<Account> GetAll(params Expression<Func<Account, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<Account> GetByIdAsync(Expression<Func<Account, bool>> predicate, params Expression<Func<Account, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(Account entity)
        {
            _iRepository.Update(entity);
        }
    }
}
