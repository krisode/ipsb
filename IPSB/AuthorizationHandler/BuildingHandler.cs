using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class BuildingHandler : AuthorizationHandler<OperationAuthorizationRequirement, Building>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Building resource)
        {
            if (context.User.IsInRole(Constants.Role.ADMIN))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            if (!context.User.IsInRole(Constants.Role.BUILDING_MANAGER))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            bool isDeleteOperation = requirement.Equals(Operations.Delete);
            if (isDeleteOperation)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            bool isUpdateOperation = requirement.Equals(Operations.Update);
            int buildingManagerId = int.Parse(context.User.Identity.Name);
            if (isUpdateOperation && !resource.ManagerId.Equals(buildingManagerId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
