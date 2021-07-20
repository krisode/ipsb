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
        Account CheckLogin(string email, string password);

        Account CheckEmail(string email);
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

        public Account CheckLogin(string email, string password)
        {
            return _iRepository.GetAllWhere(_ => _.Email.Equals(email), _ => _.Password.Equals(password)).FirstOrDefault();
        }

        public Account CheckEmail(string email)
        {
            return _iRepository.GetAllWhere(_ => _.Email.Equals(email)).FirstOrDefault();
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
