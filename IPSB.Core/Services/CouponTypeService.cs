using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;

namespace IPSB.Core.Services
{
    public class CouponTypeService : ICouponTypeService
    {
        private readonly IRepository<CouponType, int> _iRepository;

        public CouponTypeService(IRepository<CouponType, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public Task<CouponType> AddAsync(CouponType entity)
        {
            return _iRepository.AddAsync(entity);
        }

        public void Delete(CouponType entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<CouponType> GetAll(params Expression<Func<CouponType, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public Task<CouponType> GetByIdAsync(Expression<Func<CouponType, bool>> predicate, params Expression<Func<CouponType, object>>[] includes)
        {
            return _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(CouponType entity)
        {
            _iRepository.Update(entity);
        }
    }

}
public interface ICouponTypeService : IService<CouponType, int>
{

}

