using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class ProductCategoryHandler : AuthorizationHandler<OperationAuthorizationRequirement, ProductCategory>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, ProductCategory resource)
        {
            if (!context.User.IsInRole(Constants.Role.ADMIN))
            {
                context.Fail();
                return Task.CompletedTask;
            }


            bool isCreateOperation = requirement.Equals(Operations.Create);
            bool isReadOperation = requirement.Equals(Operations.Read);
            bool isDeleteOperation = requirement.Equals(Operations.Delete);
            bool isUpdateOperation = requirement.Equals(Operations.Update);

            bool needAuthorized = isCreateOperation || isReadOperation || isDeleteOperation || isUpdateOperation;

            if (needAuthorized)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
