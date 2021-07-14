using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class VisitRoute
    {
        public VisitRoute()
        {
            VisitPoints = new HashSet<VisitPoint>();
        }

        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime RecordTime { get; set; }
        public int BuildingId { get; set; }

        public virtual Account Account { get; set; }
        public virtual Building Building { get; set; }
        public virtual ICollection<VisitPoint> VisitPoints { get; set; }
    }
}
