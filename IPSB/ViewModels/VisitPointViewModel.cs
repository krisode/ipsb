using System;

namespace IPSB.ViewModels
{
    public class VisitPointRefModel
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int VisitRouteId { get; set; }
        public DateTime RecordTime { get; set; }
    }
}
