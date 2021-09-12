using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class StoreHandler : AuthorizationHandler<OperationAuthorizationRequirement, Store>
    {
       
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Store resource)
        {

            return Task.CompletedTask;
        }
    }
}
