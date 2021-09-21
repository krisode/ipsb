/*using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class ProductHandler : AuthorizationHandler<OperationAuthorizationRequirement, Product>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Product resource)
        {
            if (!context.User.IsInRole(Constants.Role.STORE_OWNER) || !context.User.IsInRole(Constants.Role.VISITOR))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            int userId = int.Parse(context.User.Identity.Name);

            if (context.User.IsInRole(Constants.Role.STORE_OWNER) && !resource.Store.Building.AdminId.Equals(userId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            bool isCreateOperation = requirement.Equals(Operations.Create);
            bool isReadOperation = requirement.Equals(Operations.Read);
            bool isDeleteOperation = requirement.Equals(Operations.Delete);
            bool isUpdateOperation = requirement.Equals(Operations.Update);

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

            return Task.CompletedTask;
        }
    }
}
*/