using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Authorization
{
    public class RequiredRolePolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => 
            Task.FromResult(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build());

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy>(null);

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PrefixPolicy.REQUIRED_ROLE, StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                var role = policyName.Substring(PrefixPolicy.REQUIRED_ROLE.Length);
                policy.AddRequirements(new RequiredRoleRequirement(role));
                return Task.FromResult(policy.Build());
            }
            return Task.FromResult<AuthorizationPolicy>(null);
        }
    }
}
