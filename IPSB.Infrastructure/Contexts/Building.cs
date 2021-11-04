using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Building
    {
        public Building()
        {
            FloorPlans = new HashSet<FloorPlan>();
            LocatorTags = new HashSet<LocatorTag>();
            ShoppingLists = new HashSet<ShoppingList>();
            Stores = new HashSet<Store>();
        }

        public int Id { get; set; }
        public int? ManagerId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public double? EnvironmentFactor { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }

        public virtual Account Manager { get; set; }
        public virtual ICollection<FloorPlan> FloorPlans { get; set; }
        public virtual ICollection<LocatorTag> LocatorTags { get; set; }
        public virtual ICollection<ShoppingList> ShoppingLists { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
    }
}
