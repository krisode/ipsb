using System;

namespace IPSB.ViewModels
{
    public class CouponInUseRefModel
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int VisitorId { get; set; }
        public DateTime RedeemDate { get; set; }
        public DateTime ApplyDate { get; set; }
        public string Status { get; set; }
    }
}
