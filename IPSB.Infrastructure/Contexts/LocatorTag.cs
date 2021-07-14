using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class LocatorTag
    {
        public int Id { get; set; }
        public string MacAddress { get; set; }
        public string Status { get; set; }
        public DateTime? UpdateTime { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationId { get; set; }
        public DateTime? LastSeen { get; set; }

        public virtual FloorPlan FloorPlan { get; set; }
        public virtual Location Location { get; set; }
    }
}
