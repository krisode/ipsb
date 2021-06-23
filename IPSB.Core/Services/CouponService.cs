using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface ICouponService : IService<Coupon, int>
    {

    }

    public class CouponService : ICouponService
    {
        private readonly IRepository<Coupon, int> _iRepository;

        public CouponService(IRepository<Coupon, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<Coupon> AddAsync(Coupon entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(Coupon entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<Coupon> GetAll(params Expression<Func<Coupon, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<Coupon> GetByIdAsync(Expression<Func<Coupon, bool>> predicate, params Expression<Func<Coupon, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(Coupon entity)
        {
            _iRepository.Update(entity);
        }
    }
}
