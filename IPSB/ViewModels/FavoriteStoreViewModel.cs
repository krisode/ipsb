using System;

namespace IPSB.ViewModels
{
    public class FavoriteStoreVM
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public DateTime RecordDate { get; set; }
        public int VisitCount { get; set; }
        public int BuildingId { get; set; }

        public StoreRefModel Store { get; set; }
    }

    public class FavoriteStoreRefModel
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public DateTime RecordDate { get; set; }
        public int VisitCount { get; set; }
        public int BuildingId { get; set; }

    }

    public class FavoriteStoreSM
    {
        public int StoreId { get; set; }
        public DateTime? LowerRecordDate { get; set; }
        public DateTime? UpperRecordDate { get; set; }
        public int LowerVisitCount { get; set; }
        public int UpperVisitCount { get; set; }
        public int BuildingId { get; set; }

    }

    public class FavoriteStoreCM
    {
        public int StoreId { get; set; }
        public int BuildingId { get; set; }
        
    }

    public class FavoriteStoreUM
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public DateTime? RecordDate { get; set; }
        public int VisitCount { get; set; }
        public int BuildingId { get; set; }
    }
}
