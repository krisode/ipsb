using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int BuildingId { get; set; }
        public int? LocationId { get; set; }
        public string Status { get; set; }
        public int FloorPlanId { get; set; }

        public virtual Building Building { get; set; }
        public virtual FloorPlan FloorPlan { get; set; }
        public virtual Location Location { get; set; }
    }
}
