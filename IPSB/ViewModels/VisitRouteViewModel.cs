using System;
using System.Collections.Generic;

namespace IPSB.ViewModels
{
    public class VisitRouteVM
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime RecordTime { get; set; }
        public int BuildingId { get; set; }

        public AccountRefModel Account { get; set; }
        public BuildingRefModel Building { get; set; }
        public ICollection<VisitPointRefModel> VisitPoints { get; set; }
    }
    public class VisitRouteRefModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime RecordTime { get; set; }
        public int BuildingId { get; set; }
    }
    public class VisitRouteSM
    {
        public int AccountId { get; set; }
        public int BuildingId { get; set; }
        public DateTime? LowerRecordTime { get; set; }
        public DateTime? UpperRecordTime { get; set; }
    }
    public class VisitRouteCM
    {
        public int AccountId { get; set; }
        public int BuildingId { get; set; }
    }
    public class VisitRouteUM
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime? RecordTime { get; set; }
        public int BuildingId { get; set; }
    }
}
