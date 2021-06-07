using System;
using System.Threading.Tasks;
using IPSB;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static IPSB.Utils.Constants;

namespace BeautyAtHome
{
    public class RequiredRoleHandler : AuthorizationHandler<RequiredRoleRequirement>
    {
        private readonly IJwtTokenProvider _jwtTokenProvider;

        public RequiredRoleHandler(IJwtTokenProvider jwtTokenProvider)
        {
            _jwtTokenProvider = jwtTokenProvider;
        }

        protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, RequiredRoleRequirement requirement)
        {

            DefaultHttpContext httpContext = (DefaultHttpContext)context.Resource;

            string jwtToken = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            try
            {
                string role = await _jwtTokenProvider.GetPayloadFromToken(jwtToken, TokenClaims.ROLE);
                if (role.Equals(requirement.Role))
                {
                    context.Succeed(requirement);
                }
            }
            catch (Exception)
            {
            }
            return Task.CompletedTask;
        }
    }
}
