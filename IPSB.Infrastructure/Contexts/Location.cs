using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Location
    {
        public Location()
        {
            EdgeFromLocations = new HashSet<Edge>();
            EdgeToLocations = new HashSet<Edge>();
            LocatorTags = new HashSet<LocatorTag>();
            VisitPoints = new HashSet<VisitPoint>();
        }

        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int? StoreId { get; set; }
        public int LocationTypeId { get; set; }
        public string Status { get; set; }

        public virtual FloorPlan FloorPlan { get; set; }
        public virtual LocationType LocationType { get; set; }
        public virtual Store Store { get; set; }
        public virtual Facility Facility { get; set; }
        public virtual ICollection<Edge> EdgeFromLocations { get; set; }
        public virtual ICollection<Edge> EdgeToLocations { get; set; }
        public virtual ICollection<LocatorTag> LocatorTags { get; set; }
        public virtual ICollection<VisitPoint> VisitPoints { get; set; }
    }
}
