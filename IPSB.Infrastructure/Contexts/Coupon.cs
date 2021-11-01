using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Coupon
    {
        public Coupon()
        {
            CouponInUses = new HashSet<CouponInUse>();
        }

        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StoreId { get; set; }
        public string Code { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public double? Amount { get; set; }
        public double? MaxDiscount { get; set; }
        public double? MinSpend { get; set; }
        public int? Limit { get; set; }
        public string Status { get; set; }
        public int? CouponTypeId { get; set; }

        public virtual CouponType CouponType { get; set; }
        public virtual Store Store { get; set; }
        public virtual ICollection<CouponInUse> CouponInUses { get; set; }
    }
}
