using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class AccountHandler : AuthorizationHandler<OperationAuthorizationRequirement, Account>
    {

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Account resource)
        {
            bool isDeleteOperation = requirement.Equals(Operations.Delete);
            bool isUpdateOperation = requirement.Equals(Operations.Update);
            if (context.User.IsInRole(Constants.Role.ADMIN))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if ((context.User.IsInRole(Constants.Role.VISITOR) || context.User.IsInRole(Constants.Role.STORE_OWNER))
                && resource.Id.ToString() == context.User.Identity.Name
                && !isDeleteOperation)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            int buildingManagerId = int.Parse(context.User.Identity.Name);
            bool needAuthorized = isDeleteOperation && isUpdateOperation;

            if (needAuthorized && resource.Store.Building.ManagerId != buildingManagerId)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
