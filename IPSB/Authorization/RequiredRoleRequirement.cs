using Microsoft.AspNetCore.Authorization;

namespace IPSB
{
    public class RequiredRoleRequirement : IAuthorizationRequirement
    {
        public string Role { get; set; }

        public RequiredRoleRequirement(string role)
        {
            Role = role;
        }
    }
}
