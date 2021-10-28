using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class LocatorTag
    {
        public LocatorTag()
        {
            InverseLocatorTagGroup = new HashSet<LocatorTag>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; }
        public DateTime UpdateTime { get; set; }
        public int? FloorPlanId { get; set; }
        public int? LocationId { get; set; }
        public int? BuildingId { get; set; }
        public int? LocatorTagGroupId { get; set; }
        public double? TxPower { get; set; }
        public string Status { get; set; }

        public virtual Building Building { get; set; }
        public virtual FloorPlan FloorPlan { get; set; }
        public virtual Location Location { get; set; }
        public virtual LocatorTag LocatorTagGroup { get; set; }
        public virtual ICollection<LocatorTag> InverseLocatorTagGroup { get; set; }
    }
}
