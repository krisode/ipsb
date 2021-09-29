using System;

namespace IPSB.ViewModels
{

    public class VisitPointVM
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int VisitRouteId { get; set; }
        public DateTime RecordTime { get; set; }

        public LocationRefModel Location { get; set; }
        public VisitRouteRefModel VisitRoute { get; set; }
    }
    public class VisitPointRefModel
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int VisitRouteId { get; set; }
        public DateTime RecordTime { get; set; }
    }
    public class VisitPointSM
    {
        public int LocationId { get; set; }
        public int VisitRouteId { get; set; }
        public int BuildingId { get; set; }

        public int LocationTypeId { get; set; }
        public int StoreId { get; set; }
        public DateTime? LowerRecordTime { get; set; }
        public DateTime? UpperRecordTime { get; set; }
    }
    public class VisitPointCM
    {
        public int LocationId { get; set; }
        public int VisitRouteId { get; set; }
    }
    public class VisitPointUM
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int VisitRouteId { get; set; }
        public DateTime? RecordTime { get; set; }
    }
}
