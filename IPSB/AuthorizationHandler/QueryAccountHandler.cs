using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class QueryAccountHandler : AuthorizationHandler<QueryAccountRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, QueryAccountRequirement requirement)
        {
            if (context.User.IsInRole(Constants.Role.ADMIN))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var httpContext = (DefaultHttpContext)context.Resource;
            StringValues value;
            httpContext.Request.Query.TryGetValue(Constants.QueryKeys.BUILDING_MANAGER_ID, out value);

            if (context.User.Identity.Name.Equals(value.ToString()))
            {
                context.Succeed(requirement);
            }
            
            return Task.CompletedTask;
        }
    }

    public class QueryAccountRequirement : IAuthorizationRequirement
    {
    }
}
