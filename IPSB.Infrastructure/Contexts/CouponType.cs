using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class CouponType
    {
        public CouponType()
        {
            Coupons = new HashSet<Coupon>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Coupon> Coupons { get; set; }
    }
}
