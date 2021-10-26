using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class LocatorTagHandler : AuthorizationHandler<OperationAuthorizationRequirement, LocatorTag>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, LocatorTag resource)
        {
            if (!context.User.IsInRole(Constants.Role.BUILDING_MANAGER))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            int userId = int.Parse(context.User.Identity.Name);

           

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
