using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class StoreHandler : AuthorizationHandler<OperationAuthorizationRequirement, Store>
    {

        private readonly IStoreService _storeService;

        public StoreHandler(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            _storeService = (IStoreService) scope.ServiceProvider.GetService(typeof(IStoreService));

        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Store resource)
        {

            bool isUpdateOperation = requirement.Equals(Operations.Update);
            int accountId = int.Parse(context.User.Identity.Name);
            if (isUpdateOperation && context.User.IsInRole(Constants.Role.BUILDING_MANAGER) && CheckBuildingManagerValid(accountId, resource))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            if (context.User.IsInRole(Constants.Role.STORE_OWNER) || context.User.IsInRole(Constants.Role.VISITOR))
            {
                int storeOwnerId = int.Parse(context.User.Identity.Name);

                if (context.User.IsInRole(Constants.Role.STORE_OWNER) && !resource.AccountId.Equals(storeOwnerId))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                bool isCreateOperation = requirement.Equals(Operations.Create);
                bool isReadOperation = requirement.Equals(Operations.Read);
                bool isDeleteOperation = requirement.Equals(Operations.Delete);


                bool needAuthorized = isCreateOperation || isReadOperation || isDeleteOperation || isUpdateOperation;

                if (isCreateOperation || isUpdateOperation || isDeleteOperation && context.User.IsInRole(Constants.Role.VISITOR))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                if (needAuthorized)
                {
                    context.Succeed(requirement);
                }
            }
            else
            {
                context.Fail();
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private bool CheckBuildingManagerValid(int managerId, Store resource)
        {
            return _storeService.GetAll().Where(_ => _.Building.ManagerId == managerId && _.Id == resource.Id).Count() >= 1;
        }
    }
}
