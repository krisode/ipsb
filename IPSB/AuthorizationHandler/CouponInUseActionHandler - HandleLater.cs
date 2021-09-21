using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace IPSB.AuthorizationHandler
{
    public class CouponInUseActionHandler : AuthorizationHandler<CouponInUseActionRequirement, CouponInUse>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CouponInUseActionRequirement requirement, CouponInUse resource)
        {
            if (context.User.IsInRole(Constants.Role.VISITOR)) {
                if (requirement.CouponInUseUM.Status != resource.Status 
                    || requirement.CouponInUseUM.FeedbackContent != resource.FeedbackContent 
                    || requirement.CouponInUseUM.FeedbackDate != resource.FeedbackDate
                    || requirement.CouponInUseUM.RateScore != resource.RateScore
                    && requirement.CouponInUseUM.ApplyDate.Equals(resource.ApplyDate)
                    && requirement.CouponInUseUM.CouponId.Equals(resource.CouponId)
                    && requirement.CouponInUseUM.VisitorId.Equals(resource.VisitorId)
                    /*&& requirement.CouponInUseUM.ImageUrl.Equals(resource.)*/
                    )
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                } else
                {
                    context.Fail();
                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }
    }

    public class CouponInUseActionRequirement : IAuthorizationRequirement
    {
        public CouponInUseUM CouponInUseUM { get; set; }

        public CouponInUseActionRequirement(CouponInUseUM couponInUseUM)
        {
            CouponInUseUM = couponInUseUM;
        }
    }
}
