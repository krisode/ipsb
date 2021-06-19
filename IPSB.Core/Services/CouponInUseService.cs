using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface ICouponInUseService : IService<CouponInUse, int>
    {
        IQueryable<CouponInUse> GetAllWhere(params Expression<Func<CouponInUse, bool>>[] includes);
    }

    public class CouponInUseService : ICouponInUseService
    {
        private readonly IRepository<CouponInUse, int> _iRepository;

        public CouponInUseService(IRepository<CouponInUse, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<CouponInUse> AddAsync(CouponInUse entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(CouponInUse entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<CouponInUse> GetAll(params Expression<Func<CouponInUse, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }
        
        public IQueryable<CouponInUse> GetAllWhere(params Expression<Func<CouponInUse, bool>>[] includes)
        {
            return _iRepository.GetAllWhere(includes);
        }

        public async Task<CouponInUse> GetByIdAsync(Expression<Func<CouponInUse, bool>> predicate, params Expression<Func<CouponInUse, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(CouponInUse entity)
        {
            _iRepository.Update(entity);
        }
    }
}
