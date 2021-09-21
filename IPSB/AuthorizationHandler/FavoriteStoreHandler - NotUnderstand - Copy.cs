/*using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class EdgeHandler : AuthorizationHandler<OperationAuthorizationRequirement, Edge>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Edge resource)
        {
            if (!context.User.IsInRole(Constants.Role.BUILDING_MANAGER) || !context.User.IsInRole(Constants.Role.VISITOR))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            int buildingManagerId = int.Parse(context.User.Identity.Name);

            if (context.User.IsInRole(Constants.Role.BUILDING_MANAGER) && !resource.FromLocation.FloorPlan.Building.AdminId.Equals(buildingManagerId))
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