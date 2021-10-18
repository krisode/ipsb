using ApplicationCore.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPSB.Core.Services
{
    public interface INotificationService : IService<Notification, int>
    {

    }

    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification, int> _iRepository;

        public NotificationService(IRepository<Notification, int> iRepository)
        {
            _iRepository = iRepository;
        }

        public async Task<Notification> AddAsync(Notification entity)
        {
            return await _iRepository.AddAsync(entity);
        }

        public void Delete(Notification entity)
        {
            _iRepository.Delete(entity);
        }

        public IQueryable<Notification> GetAll(params Expression<Func<Notification, object>>[] includes)
        {
            return _iRepository.GetAll(includes);
        }

        public async Task<Notification> GetByIdAsync(Expression<Func<Notification, bool>> predicate, params Expression<Func<Notification, object>>[] includes)
        {
            return await _iRepository.GetByIdAsync(predicate, includes);
        }

        public Task<int> Save()
        {
            return _iRepository.Save();
        }

        public void Update(Notification entity)
        {
            _iRepository.Update(entity);
        }
    }
}
