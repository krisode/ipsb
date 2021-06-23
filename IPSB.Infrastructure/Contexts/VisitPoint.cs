using System;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class VisitPoint
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int VisitRouteId { get; set; }
        public DateTime RecordTime { get; set; }

        public virtual Location Location { get; set; }
        public virtual VisitRoute VisitRoute { get; set; }
    }
}
