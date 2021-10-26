using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class FloorPlan
    {
        public FloorPlan()
        {
            Facilities = new HashSet<Facility>();
            Locations = new HashSet<Location>();
            LocatorTags = new HashSet<LocatorTag>();
            Stores = new HashSet<Store>();
        }

        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string FloorCode { get; set; }
        public double? MapScale { get; set; }
        public int FloorNumber { get; set; }
        public double? RotationAngle { get; set; }
        public string Status { get; set; }

        public virtual Building Building { get; set; }
        public virtual ICollection<Facility> Facilities { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
        public virtual ICollection<LocatorTag> LocatorTags { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
    }
}
