using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace IPSB.AuthorizationHandler
{
    public class AccountHandler : AuthorizationHandler<OperationAuthorizationRequirement, Account>
    {

        private readonly IStoreService _storeService;

        public AccountHandler(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            _storeService = (IStoreService)scope.ServiceProvider.GetService(typeof(IStoreService));

        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Account resource)
        {


            int accountId;
            int.TryParse(context.User.Identity.Name, out accountId);
            bool isCreateOperation = requirement.Equals(Operations.Delete);
            bool isUpdateOperation = requirement.Equals(Operations.Update);
            bool isDeleteOperation = requirement.Equals(Operations.Delete);

            // Pass the authorize handler if user request their owned account
            if (accountId == resource.Id && !isDeleteOperation)
            {
                context.Succeed(requirement);
                return;
            }

            // Fail the authorize handler if user is of role [Visitor] or [Store Owner]
            if (context.User.IsInRole(Constants.Role.VISITOR) || context.User.IsInRole(Constants.Role.STORE_OWNER))
            {
                context.Fail();
                return;
            }




            // Create operation
            if (isCreateOperation)
            {
                // Fail if create account not building manager
                if (context.User.IsInRole(Constants.Role.ADMIN) && resource.Role == Constants.Role.BUILDING_MANAGER)
                {
                    context.Succeed(requirement);
                    return;
                }
                // Fail if create account not store owner
                if (context.User.IsInRole(Constants.Role.BUILDING_MANAGER) && resource.Role == Constants.Role.STORE_OWNER)
                {
                    context.Succeed(requirement);
                    return;
                }
            }


            if (isUpdateOperation || isDeleteOperation)
            {
                if (context.User.IsInRole(Constants.Role.ADMIN) && resource.Role == Constants.Role.BUILDING_MANAGER)
                {
                    context.Succeed(requirement);
                    return;
                }
                if (context.User.IsInRole(Constants.Role.BUILDING_MANAGER) && await IsStoreManagedByManager(resource.Id, accountId))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            context.Fail();
        }
        Task<bool> IsStoreManagedByManager(int storeOwnerId, int managerId)
        {
            var recordFound = _storeService.GetAll(_ => _.Building.ManagerId == managerId && _.Account.Id == storeOwnerId).Count();
            return Task.FromResult(recordFound == 1);
        }
    }


}
