using System;

namespace IPSB.ViewModels
{

    public class VisitStoreVM
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public DateTime RecordTime { get; set; }

        public StoreRefModel Store { get; set; }
    }
    public class VisitStoreRefModel
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public DateTime RecordTime { get; set; }
    }
    public class VisitStoreSM
    {
        public int BuildingId { get; set; }
        public int StoreId { get; set; }
        public DateTime? LowerRecordTime { get; set; }
        public DateTime? UpperRecordTime { get; set; }
    }
    public class VisitStoreCM
    {
        public int StoreId { get; set; }
    }
    public class VisitStoreUM
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public DateTime? RecordTime { get; set; }
    }
}
