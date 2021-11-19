using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Building
    {
        public Building()
        {
            Accounts = new HashSet<Account>();
            Facilities = new HashSet<Facility>();
            FloorPlans = new HashSet<FloorPlan>();
            LocatorTags = new HashSet<LocatorTag>();
            ShoppingLists = new HashSet<ShoppingList>();
            Stores = new HashSet<Store>();
        }

        public int Id { get; set; }
        public int? ManagerId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Status { get; set; }

        public virtual Account Manager { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Facility> Facilities { get; set; }
        public virtual ICollection<FloorPlan> FloorPlans { get; set; }
        public virtual ICollection<LocatorTag> LocatorTags { get; set; }
        public virtual ICollection<ShoppingList> ShoppingLists { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
    }
}
