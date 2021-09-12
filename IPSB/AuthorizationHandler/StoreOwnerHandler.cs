using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class StoreOwnerHandler : AuthorizationHandler<OperationAuthorizationRequirement, Account>
    {
       
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Account resource)
        {
            if (context.User.IsInRole(Constants.Role.ADMIN))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            int buildingManagerId = int.Parse(context.User.Identity.Name);

            bool isReadOperation = requirement.Equals(Operations.Read);
            bool isDeleteOperation = requirement.Equals(Operations.Delete);
            bool isUpdateOperation = requirement.Equals(Operations.Update);

            bool needAuthorized = isReadOperation && isDeleteOperation && isUpdateOperation;

            if (needAuthorized && resource.Stores.All(_ => _.Building.ManagerId == buildingManagerId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
