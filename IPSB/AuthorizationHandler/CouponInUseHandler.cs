using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class CouponInUseHandler : AuthorizationHandler<OperationAuthorizationRequirement, CouponInUse>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, CouponInUse resource)
        {
            if (context.User.IsInRole(Constants.Role.STORE_OWNER) || context.User.IsInRole(Constants.Role.VISITOR))
            {
                int userId = int.Parse(context.User.Identity.Name);

                if (!resource.VisitorId.Equals(userId) || !resource.Coupon.Store.AccountId.Equals(userId))
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

            }
            else
            {
                context.Fail();
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
