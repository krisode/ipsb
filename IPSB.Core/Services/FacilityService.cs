using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;

namespace IPSB.Core.Services
{

    public interface IFacilityService : IService<Facility, int>
    {

    }

    public class FacilityService : IFacilityService

    {
        private readonly IRepository<Facility, int> _iRepository;

        public FacilityService(IRepository<Facility, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public Task<Facility> AddAsync(Facility entity)
        {
            return _iRepository.AddAsync(entity);
        }

        public void Delete(Facility entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<Facility> GetAll(params Expression<Func<Facility, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public Task<Facility> GetByIdAsync(Expression<Func<Facility, bool>> predicate, params Expression<Func<Facility, object>>[] includes)
        {
            return _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(Facility entity)
        {
            _iRepository.Update(entity);
        }
    }

}
