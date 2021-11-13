using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Store
    {
        public Store()
        {
            Coupons = new HashSet<Coupon>();
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? AccountId { get; set; }
        public string ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string Description { get; set; }
        public int? LocationId { get; set; }
        public int FloorPlanId { get; set; }
        public string ProductCategoryIds { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }

        public virtual Account Account { get; set; }
        public virtual Building Building { get; set; }
        public virtual FloorPlan FloorPlan { get; set; }
        public virtual Location Location { get; set; }
        public virtual ICollection<Coupon> Coupons { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
