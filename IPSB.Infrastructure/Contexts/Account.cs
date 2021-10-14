﻿using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Account
    {
        public Account()
        {
            BuildingAdmins = new HashSet<Building>();
            CouponInUses = new HashSet<CouponInUse>();
            ShoppingLists = new HashSet<ShoppingList>();
            VisitRoutes = new HashSet<VisitRoute>();
        }

        public int Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }

        public virtual Building BuildingManager { get; set; }
        public virtual Store Store { get; set; }
        public virtual ICollection<Building> BuildingAdmins { get; set; }
        public virtual ICollection<CouponInUse> CouponInUses { get; set; }
        public virtual ICollection<ShoppingList> ShoppingLists { get; set; }
        public virtual ICollection<VisitRoute> VisitRoutes { get; set; }
    }
}
