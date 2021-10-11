using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class CouponInUse
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int VisitorId { get; set; }
        public DateTime RedeemDate { get; set; }
        public DateTime? ApplyDate { get; set; }
        public string Status { get; set; }
        public string FeedbackContent { get; set; }
        public string FeedbackImage { get; set; }
        public string FeedbackReply { get; set; }
        public DateTime? FeedbackDate { get; set; }
        public double? RateScore { get; set; }

        public virtual Coupon Coupon { get; set; }
        public virtual Account Visitor { get; set; }
    }
}
