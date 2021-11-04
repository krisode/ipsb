using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Account
    {
        public Account()
        {
            CouponInUses = new HashSet<CouponInUse>();
            Notifications = new HashSet<Notification>();
            ShoppingLists = new HashSet<ShoppingList>();
        }

        public int Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }

        public virtual Building Building { get; set; }
        public virtual Store Store { get; set; }
        public virtual ICollection<CouponInUse> CouponInUses { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<ShoppingList> ShoppingLists { get; set; }
    }
}
